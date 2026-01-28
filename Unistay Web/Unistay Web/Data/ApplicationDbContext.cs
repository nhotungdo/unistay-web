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
        public DbSet<Connection> Connections { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Customize Identity table names if needed
            builder.Entity<UserProfile>().ToTable("UserProfiles");

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
        }
    }
}
