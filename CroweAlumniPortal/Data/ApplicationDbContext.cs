using CroweAlumniPortal.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace CroweAlumniPortal.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Event> Events {  get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostLike> PostLikes { get; set; }
        public DbSet<PostComment> PostComments { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NotificationUser> NotificationUsers { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<ConversationMember> ConversationMembers { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.EmailAddress)
                .IsUnique();


            modelBuilder.Entity<User>()
                .HasIndex(u => u.LoginId)
                .IsUnique();
            
            modelBuilder.Entity<User>()
              .HasIndex(u => u.CNIC)
              .IsUnique();

            modelBuilder.Entity<ConversationMember>()
                .HasKey(x => new { x.ConversationId, x.UserId });

            modelBuilder.Entity<ConversationMember>()
                .HasOne(x => x.Conversation)
                .WithMany(c => c.Members)
                .HasForeignKey(x => x.ConversationId);

            modelBuilder.Entity<ConversationMember>()
                .HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId);
        }

    }
}
