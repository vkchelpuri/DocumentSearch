using DocumentFormat.OpenXml.InkML;
using DocumentQnA.Api.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
namespace DocumentQnA.Api.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser> // Changed base class
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Existing DbSets for your application data
        public DbSet<DocumentText> Documents { get; set; }
        public DbSet<DocumentText> DocumentTexts { get; set; } // Consider if you need both 'Documents' and 'DocumentTexts'
        public DbSet<ChatHistory> ChatHistories { get; set; }

        /// <summary>
        /// Configures the model that was discovered by the convention from the entity types
        /// found in the context. This method is called once when the model for a context
        /// is first created.
        /// </summary>
        /// <param name="builder">The builder being used to construct the model for this context.</param>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // IMPORTANT: Call base method for Identity tables

            // Configure the relationship between ChatHistory and ApplicationUser
            builder.Entity<ChatHistory>()
                .HasOne(ch => ch.User)              // A ChatHistory has one User
                .WithMany(u => u.ChatHistories)     // A User can have many ChatHistories
                .HasForeignKey(ch => ch.UserId)     // The foreign key is UserId in ChatHistory
                .OnDelete(DeleteBehavior.Cascade);  // If a user is deleted, their chat history is also deleted
        }
    }

}
