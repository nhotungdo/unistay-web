using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Unistay_Web.Models.User;
using Unistay_Web.Models.Room;
using Unistay_Web.Models.Review;
using Unistay_Web.Models.Booking;
using Unistay_Web.Models.Roommate;
using Unistay_Web.Models.Marketplace;
using Unistay_Web.Models.Moving;
using Unistay_Web.Models.Service;
using Unistay_Web.Models.Payment;
using Unistay_Web.Models.Report;
using Unistay_Web.Models.Connection;

namespace Unistay_Web.Data
{
    public class ApplicationDbContext : IdentityDbContext<UserProfile>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Room> Rooms { get; set; }
        public DbSet<RoomImage> RoomImages { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<RoommateProfile> RoommateProfiles { get; set; }
        public DbSet<MarketplaceItem> MarketplaceItems { get; set; }
        public DbSet<MovingRequest> MovingRequests { get; set; }
        public DbSet<RoomFinderRequest> RoomFinderRequests { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<LoginHistory> LoginHistories { get; set; }
        public DbSet<ActivityHistory> ActivityHistories { get; set; }
        
        // Messaging & Connection
        public DbSet<Connection> Connections { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<MessageAttachment> MessageAttachments { get; set; }
        public DbSet<ChatGroup> ChatGroups { get; set; }
        public DbSet<ChatGroupMember> ChatGroupMembers { get; set; }
        public DbSet<BlockedUser> BlockedUsers { get; set; }
        public DbSet<MessageReport> MessageReports { get; set; }
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Customize Identity table names if needed
            builder.Entity<UserProfile>().ToTable("UserProfiles");

            // Connection relationships
            builder.Entity<Connection>()
                .HasOne(c => c.Requester)
                .WithMany()
                .HasForeignKey(c => c.RequesterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Connection>()
                .HasOne(c => c.Addressee)
                .WithMany()
                .HasForeignKey(c => c.AddresseeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Message relationships
            builder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Message>()
                .HasOne(m => m.Receiver)
                .WithMany()
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            // Self-referencing for reply feature
            builder.Entity<Message>()
                .HasOne(m => m.ReplyToMessage)
                .WithMany()
                .HasForeignKey(m => m.ReplyToMessageId)
                .OnDelete(DeleteBehavior.Restrict);

            // MessageAttachment relationships
            builder.Entity<MessageAttachment>()
                .HasOne(ma => ma.Message)
                .WithMany(m => m.Attachments)
                .HasForeignKey(ma => ma.MessageId)
                .OnDelete(DeleteBehavior.Cascade);

            // BlockedUser relationships
            builder.Entity<BlockedUser>()
                .HasOne(bu => bu.Blocker)
                .WithMany()
                .HasForeignKey(bu => bu.BlockerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<BlockedUser>()
                .HasOne(bu => bu.BlockedUserProfile)
                .WithMany()
                .HasForeignKey(bu => bu.BlockedUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // MessageReport relationships
            builder.Entity<MessageReport>()
                .HasOne(mr => mr.Reporter)
                .WithMany()
                .HasForeignKey(mr => mr.ReporterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<MessageReport>()
                .HasOne(mr => mr.Message)
                .WithMany()
                .HasForeignKey(mr => mr.MessageId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes for performance
            builder.Entity<Message>()
                .HasIndex(m => new { m.SenderId, m.ReceiverId });
            
            builder.Entity<Message>()
                .HasIndex(m => m.ChatGroupId);

            builder.Entity<Message>()
                .HasIndex(m => m.CreatedAt);

            builder.Entity<BlockedUser>()
                .HasIndex(bu => new { bu.BlockerId, bu.BlockedUserId })
                .IsUnique();

            // Configure Decimal properties
            builder.Entity<Room>(entity =>
            {
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Deposit).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Area).HasColumnType("decimal(18,2)");
            });

            builder.Entity<RoommateProfile>(entity =>
            {
                entity.Property(e => e.Budget).HasColumnType("decimal(18,2)");
            });

            builder.Entity<MarketplaceItem>(entity =>
            {
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            });

            builder.Entity<MovingRequest>(entity =>
            {
                entity.Property(e => e.EstimatedCost).HasColumnType("decimal(18,2)");
            });

            builder.Entity<RoomFinderRequest>(entity =>
            {
                entity.Property(e => e.Budget).HasColumnType("decimal(18,2)");
                entity.Property(e => e.ServiceFee).HasColumnType("decimal(18,2)");
            });

            builder.Entity<Transaction>(entity =>
            {
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            });

            builder.Entity<UserProfile>(entity =>
            {
                entity.Property(e => e.Budget).HasColumnType("decimal(18,2)");
                entity.Property(e => e.AverageRating).HasColumnType("decimal(18,2)");
            });
        }
    }
}
