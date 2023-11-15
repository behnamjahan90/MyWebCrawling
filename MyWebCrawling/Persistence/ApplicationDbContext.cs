
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MyWebCrawling.Core.Models;


namespace MyWebCrawling.Persistence
{
    public class ApplicationDbContext: DbContext, IApplicationDbContext
    {
        public DbSet<SearchJob> SearchJobs { get; set; }
        public DbSet<SearchResult> SearchResults { get; set; }
        public DbSet<Result> Results { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }


       
    }
}
