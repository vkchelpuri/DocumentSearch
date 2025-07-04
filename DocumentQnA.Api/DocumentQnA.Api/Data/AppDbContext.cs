using DocumentFormat.OpenXml.InkML;
using DocumentQnA.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
namespace DocumentQnA.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<DocumentText> Documents { get; set; }
        public DbSet<DocumentText> DocumentTexts { get; set; }

    }

}
