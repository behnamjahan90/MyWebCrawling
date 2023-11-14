
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MyWebCrawling.Core.Models;


namespace MyWebCrawling.Persistence
{
    public class ApplicationDbContext: DbContext
    {
        public DbSet<SearchJob> SearchJobs { get; set; }
        public DbSet<SearchResult> SearchResults { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
