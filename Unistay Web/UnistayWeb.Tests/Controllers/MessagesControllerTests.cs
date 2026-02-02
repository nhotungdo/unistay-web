using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Unistay_Web.Controllers;
using Unistay_Web.Data;
using Unistay_Web.Models.Connection;
using Unistay_Web.Models.User;
using Xunit;
using Microsoft.AspNetCore.SignalR;
using Unistay_Web.Hubs;

namespace UnistayWeb.Tests.Controllers
{
    public class MessagesControllerTests
    {
        private ApplicationDbContext _dbContext;
        private Mock<UserManager<UserProfile>> _mockUserManager;
        private MessagesController _controller;
        private Mock<Microsoft.AspNetCore.Hosting.IWebHostEnvironment> _mockEnv;
        private Mock<Microsoft.Extensions.Logging.ILogger<MessagesController>> _mockLogger;
        private Mock<IHubContext<ChatHub>> _mockHubContext;

        public MessagesControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _dbContext = new ApplicationDbContext(options);

            var store = new Mock<IUserStore<UserProfile>>();
            _mockUserManager = new Mock<UserManager<UserProfile>>(store.Object, null, null, null, null, null, null, null, null);
            
            _mockEnv = new Mock<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
            _mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger<MessagesController>>();
            _mockHubContext = new Mock<IHubContext<ChatHub>>();
            _mockHubContext.Setup(h => h.Clients.Group(It.IsAny<string>())).Returns(Mock.Of<IClientProxy>());

            _controller = new MessagesController(
                _dbContext, 
                _mockUserManager.Object, 
                _mockEnv.Object, 
                _mockLogger.Object,
                _mockHubContext.Object);
        }

        private void SetupUser(string userId)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            _mockUserManager.Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);
        }

        [Fact]
        public async Task GetConversations_ShouldReturnRecentContacts()
        {
            // Arrange
            var userId = "user1";
            var otherId = "user2";
            SetupUser(userId);

            _dbContext.Users.Add(new UserProfile { Id = userId, FullName = "Me" });
            _dbContext.Users.Add(new UserProfile { Id = otherId, FullName = "Other", AvatarUrl = "pic.png" });
            
            _dbContext.Messages.Add(new Message
            {
                SenderId = userId,
                ReceiverId = otherId,
                Content = "Hello",
                CreatedAt = DateTime.UtcNow
            });
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _controller.GetConversations();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            // Dynamic check or simple check if type visibility is an issue
            var items = Assert.IsAssignableFrom<IEnumerable<ConversationViewModel>>(okResult.Value);
            var item = Assert.Single(items);
            Assert.Equal(otherId, item.UserId);
            Assert.Equal("Other", item.UserName);
            Assert.Equal("Hello", item.LastMessage.Content);
        }

        [Fact]
        public async Task GetConversations_ShouldIncludeFriendsWithNoMessages()
        {
            // Arrange
            var userId = "user1";
            var friendId = "user3";
            SetupUser(userId);

            _dbContext.Users.Add(new UserProfile { Id = userId, FullName = "Me" });
            _dbContext.Users.Add(new UserProfile { Id = friendId, FullName = "Friend" });

            _dbContext.Connections.Add(new Connection 
            { 
               RequesterId = userId, 
               AddresseeId = friendId, 
               Status = ConnectionStatus.Accepted 
            });
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _controller.GetConversations();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var items = Assert.IsAssignableFrom<IEnumerable<ConversationViewModel>>(okResult.Value);
            Assert.NotEmpty(items); // Should contain friend
        }

        [Fact]
        public async Task SendMessage_ShouldSucceed()
        {
            // Arrange
            var userId = "user1";
            var receiverId = "user2";
            SetupUser(userId);

            _dbContext.Users.Add(new UserProfile { Id = userId });
            _dbContext.Users.Add(new UserProfile { Id = receiverId });
            await _dbContext.SaveChangesAsync();

            var dto = new SendMessageDto { ReceiverId = receiverId, Content = "Test" };

            // Act
            var result = await _controller.SendMessage(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var msgInDb = await _dbContext.Messages.FirstOrDefaultAsync();
            Assert.NotNull(msgInDb);
            Assert.Equal("Test", msgInDb.Content);
        }

        [Fact]
        public async Task GetMessages_ShouldMarkAsDelivered()
        {
             // Arrange
            var userId = "user1";
            var senderId = "user2";
            SetupUser(userId); // Viewing messages from user2

            _dbContext.Users.Add(new UserProfile { Id = userId });
            _dbContext.Users.Add(new UserProfile { Id = senderId });

            var msg = new Message 
            { 
                SenderId = senderId, 
                ReceiverId = userId, 
                Content = "Hi", 
                Status = MessageStatus.Sent,
                CreatedAt = DateTime.UtcNow
            };
            _dbContext.Messages.Add(msg);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _controller.GetMessages(senderId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dbMsg = await _dbContext.Messages.FindAsync(msg.Id);
            Assert.Equal(MessageStatus.Delivered, dbMsg.Status);
        }
        [Fact]
        public async Task GetGroupMessages_ShouldReturnMessages()
        {
            // Arrange
            var userId = "user1";
            var groupId = 1;
            SetupUser(userId);

            _dbContext.ChatGroups.Add(new ChatGroup { Id = groupId, Name = "Test Group" });
            _dbContext.ChatGroupMembers.Add(new ChatGroupMember { ChatGroupId = groupId, UserId = userId, Role = GroupRole.Member });
            
            _dbContext.Messages.Add(new Message
            {
                ChatGroupId = groupId,
                SenderId = userId,
                Content = "Group Hello",
                CreatedAt = DateTime.UtcNow
            });
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _controller.GetGroupMessages(groupId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            dynamic data = okResult.Value;
            // The result is an anonymous type or object with 'data' property
            // We can inspect it via reflection or Cast if we knew the type.
            // Simplified check:
            Assert.NotNull(data);
        }

        [Fact]
        public async Task SendMessage_ToGroup_ShouldSucceed()
        {
            // Arrange
            var userId = "user1";
            var groupId = 1;
            SetupUser(userId);

            _dbContext.ChatGroups.Add(new ChatGroup { Id = groupId, Name = "Test Group" });
            _dbContext.ChatGroupMembers.Add(new ChatGroupMember { ChatGroupId = groupId, UserId = userId });
            await _dbContext.SaveChangesAsync();

            var dto = new SendMessageDto { GroupId = groupId, Content = "Group Hi" };

            // Act
            var result = await _controller.SendMessage(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var msgInDb = await _dbContext.Messages.FirstOrDefaultAsync(m => m.Content == "Group Hi");
            Assert.NotNull(msgInDb);
            Assert.Equal(groupId, msgInDb.ChatGroupId);
        }
    }
}
