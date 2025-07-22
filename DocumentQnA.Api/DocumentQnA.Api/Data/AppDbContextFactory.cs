// Data/AppDbContextFactory.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace DocumentQnA.Api.Data
{
    /// <summary>
    /// A factory for creating AppDbContext instances at design time.
    /// This is necessary for Entity Framework Core CLI commands like Add-Migration and Update-Database.
    /// </summary>
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        /// <summary>
        /// Creates a new instance of AppDbContext for design-time operations.
        /// </summary>
        /// <param name="args">Command line arguments (not typically used in this context).</param>
        /// <returns>A new instance of AppDbContext.</returns>
        public AppDbContext CreateDbContext(string[] args)
        {
            // Build configuration from appsettings.json
            // This allows the migration tools to read your connection string.
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // Set base path to the project directory
                .AddJsonFile("appsettings.json") // Load appsettings.json
                .Build();

            // Get the connection string
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // Configure DbContextOptions
            var builder = new DbContextOptionsBuilder<AppDbContext>();
            builder.UseSqlServer(connectionString);

            // Return a new instance of AppDbContext with the configured options
            return new AppDbContext(builder.Options);
        }
    }
}
