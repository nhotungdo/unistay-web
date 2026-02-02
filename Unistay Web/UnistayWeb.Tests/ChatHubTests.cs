using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Unistay_Web.Data;
using Unistay_Web.Hubs;
using Unistay_Web.Models.Connection;
using Xunit;

namespace UnistayWeb.Tests
{
    public class ChatHubTests
    {
        private Mock<IHubCallerClients> _mockClients;
        private Mock<IClientProxy> _mockClientProxy;
        private Mock<HubCallerContext> _mockContext;
        private Mock<IGroupManager> _mockGroups;
        private ApplicationDbContext _dbContext;
        private ChatHub _hub;

        public ChatHubTests()
        {
            // Setup In-Memory DB
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _dbContext = new ApplicationDbContext(options);

            // Setup Mocks
            _mockClients = new Mock<IHubCallerClients>();
            _mockClientProxy = new Mock<IClientProxy>();
            _mockContext = new Mock<HubCallerContext>();
            _mockGroups = new Mock<IGroupManager>();

            _mockClients.Setup(clients => clients.Group(It.IsAny<string>())).Returns(_mockClientProxy.Object);
            _mockClients.Setup(clients => clients.Groups(It.IsAny<IReadOnlyList<string>>())).Returns(_mockClientProxy.Object);

            _mockGroups.Setup(g => g.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _mockGroups.Setup(g => g.RemoveFromGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockClientProxy.Setup(c => c.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _hub = new ChatHub(_dbContext)
            {
                Clients = _mockClients.Object,
                Context = _mockContext.Object,
                Groups = _mockGroups.Object
            };
        }

        [Fact]
        public async Task OnConnectedAsync_ShouldJoinGroups_AndNotifyFriends()
        {
            // Arrange
            var userId = "user1";
            var friendId = "user2";
            var groupId = 10;
            
            _mockContext.Setup(c => c.UserIdentifier).Returns(userId);
            _mockContext.Setup(c => c.ConnectionId).Returns("conn1");

            // Seed DB
            _dbContext.ChatGroupMembers.Add(new ChatGroupMember { ChatGroupId = groupId, UserId = userId, Role = GroupRole.Member });
            _dbContext.Connections.Add(new Connection { RequesterId = userId, AddresseeId = friendId, Status = ConnectionStatus.Accepted });
            await _dbContext.SaveChangesAsync();

            // Act
            await _hub.OnConnectedAsync();

            // Assert
            // 1. Should join personal group
            _mockGroups.Verify(g => g.AddToGroupAsync("conn1", userId, default), Times.Once);
            // 2. Should join chat group
            _mockGroups.Verify(g => g.AddToGroupAsync("conn1", $"group_{groupId}", default), Times.Once);
            // 3. Should notify friends
            _mockClients.Verify(c => c.Groups(It.Is<IReadOnlyList<string>>(l => l.Contains(friendId))), Times.Once);
             _mockClientProxy.Verify(c => c.SendCoreAsync("UserOnline", It.IsAny<object[]>(), default), Times.Once);
        }

        [Fact]
        public async Task SendMessage_OneOnOne_Success_ShouldSaveAndBroadcast()
        {
            // Arrange
            var senderId = "user1";
            var receiverId = "user2";
            var content = "Hello World";
            
            _mockContext.Setup(c => c.UserIdentifier).Returns(senderId);

            _dbContext.Connections.Add(new Connection 
            { 
                RequesterId = senderId, 
                AddresseeId = receiverId, 
                Status = ConnectionStatus.Accepted 
            });
            await _dbContext.SaveChangesAsync();

            // Act
            await _hub.SendMessage(receiverId, null, content);

            // Assert
            var savedMessage = await _dbContext.Messages.FirstOrDefaultAsync();
            Assert.NotNull(savedMessage);
            Assert.Equal(content, savedMessage.Content);
            Assert.Equal(senderId, savedMessage.SenderId);
            Assert.Equal(receiverId, savedMessage.ReceiverId);

            _mockClients.Verify(clients => clients.Group(receiverId), Times.Once);
            _mockClientProxy.Verify(c => c.SendCoreAsync("ReceiveMessage", It.IsAny<object[]>(), default), Times.Once);
        }

        [Fact]
        public async Task SendMessage_Group_Success_ShouldSaveAndBroadcast()
        {
            // Arrange
            var senderId = "user1";
            var groupId = 100;
            var content = "Group Hello";
            
            _mockContext.Setup(c => c.UserIdentifier).Returns(senderId);

            _dbContext.ChatGroupMembers.Add(new ChatGroupMember { ChatGroupId = groupId, UserId = senderId, Role = GroupRole.Member });
            await _dbContext.SaveChangesAsync();

            // Act
            await _hub.SendMessage(null, groupId, content);

            // Assert
            var savedMessage = await _dbContext.Messages.FirstOrDefaultAsync();
            Assert.NotNull(savedMessage);
            Assert.Equal(content, savedMessage.Content);
            Assert.Equal(groupId, savedMessage.ChatGroupId);

            _mockClients.Verify(clients => clients.Group($"group_{groupId}"), Times.Once);
            _mockClientProxy.Verify(c => c.SendCoreAsync("ReceiveMessage", It.IsAny<object[]>(), default), Times.Once);
        }

        [Fact]
        public async Task SendMessage_NotFriend_ShouldSend()
        {
             // Arrange
            var senderId = "user1";
            var receiverId = "user2"; // Not friend
            
            _mockContext.Setup(c => c.UserIdentifier).Returns(senderId);

            // Act
            await _hub.SendMessage(receiverId, null, "Hi");

            // Assert
            var savedMessage = await _dbContext.Messages.FirstOrDefaultAsync();
            Assert.NotNull(savedMessage);
            Assert.Equal("Hi", savedMessage.Content);
            
            _mockClients.Verify(clients => clients.Group(receiverId), Times.Once);
        }

        [Fact]
        public async Task SendMessage_InvalidType_ShouldDefaultToText()
        {
             // Arrange
            var senderId = "user1";
            var receiverId = "user2";
            
            _mockContext.Setup(c => c.UserIdentifier).Returns(senderId);

             _dbContext.Connections.Add(new Connection { RequesterId = senderId, AddresseeId = receiverId, Status = ConnectionStatus.Accepted });
            await _dbContext.SaveChangesAsync();

            // Act
            // Passing "InvalidType" which would crash Enum.Parse if not handled
            await _hub.SendMessage(receiverId, null, "Hi", null, "InvalidType");

            // Assert
            var msg = await _dbContext.Messages.FirstOrDefaultAsync();
            Assert.NotNull(msg);
            Assert.Equal(MessageType.Text, msg.Type);
        }

        [Fact]
        public async Task CreateGroup_ShouldCreateGroupAndAddMember()
        {
            // Arrange
            var senderId = "user1";
            var memberId = "user2";
            var groupName = "Test Group";

            _mockContext.Setup(c => c.UserIdentifier).Returns(senderId);

            // Act
            await _hub.CreateGroup(groupName, new List<string> { memberId });

            // Assert
            var group = await _dbContext.ChatGroups.FirstOrDefaultAsync();
            Assert.NotNull(group);
            Assert.Equal(groupName, group.Name);

            var members = await _dbContext.ChatGroupMembers.Where(m => m.ChatGroupId == group.Id).ToListAsync();
            Assert.Equal(2, members.Count); // Sender + Member
            Assert.Contains(members, m => m.UserId == senderId && m.Role == GroupRole.Admin);
        }

        [Fact]
        public async Task Typing_ShouldNotifyReceiver()
        {
             // Arrange
            var senderId = "user1";
            var receiverId = "user2";
            _mockContext.Setup(c => c.UserIdentifier).Returns(senderId);

            // Act
            await _hub.Typing(receiverId, null);

            // Assert
            _mockClients.Verify(c => c.Group(receiverId), Times.Once);
            _mockClientProxy.Verify(c => c.SendCoreAsync("UserTyping", It.Is<object[]>(o => o[0].ToString() == senderId), default), Times.Once);
        }

        [Fact]
        public async Task MessageRead_ShouldUpdateStatus_AndNotifySender()
        {
             // Arrange
            var senderId = "user1";
            var receiverId = "user2"; // Me
            
            _mockContext.Setup(c => c.UserIdentifier).Returns(receiverId);

            var msg = new Message {
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = "Hi",
                Status = MessageStatus.Sent
            };
            _dbContext.Messages.Add(msg);
            await _dbContext.SaveChangesAsync();

            // Act
            await _hub.MessageRead(msg.Id);

            // Assert
            var dbMsg = await _dbContext.Messages.FindAsync(msg.Id);
            Assert.Equal(MessageStatus.Seen, dbMsg.Status);

             _mockClients.Verify(c => c.Group(senderId), Times.Once);
             _mockClientProxy.Verify(c => c.SendCoreAsync("MessageSeen", It.Is<object[]>(o => (int)o[0] == msg.Id), default), Times.Once);
        }

        [Fact]
        public async Task OnDisconnectedAsync_ShouldLeaveGroups_AndNotifyFriends()
        {
            // Arrange
            var userId = "user1";
            var friendId = "user2";
             _mockContext.Setup(c => c.UserIdentifier).Returns(userId);
             _mockContext.Setup(c => c.ConnectionId).Returns("conn1");

             _dbContext.Connections.Add(new Connection { RequesterId = userId, AddresseeId = friendId, Status = ConnectionStatus.Accepted });
             await _dbContext.SaveChangesAsync();

            // Act
            await _hub.OnDisconnectedAsync(null);

            // Assert
            _mockGroups.Verify(g => g.RemoveFromGroupAsync("conn1", userId, default), Times.Once);
            
            _mockClients.Verify(c => c.Groups(It.Is<IReadOnlyList<string>>(l => l.Contains(friendId))), Times.Once);
            _mockClientProxy.Verify(c => c.SendCoreAsync("UserOffline", It.Is<object[]>(o => o[0].ToString() == userId), default), Times.Once);
        }
    }
}
