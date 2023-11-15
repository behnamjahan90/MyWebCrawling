using MyWebCrawling.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace MyWebCrawling.Persistence
{
    public interface IApplicationDbContext
    {
         DbSet<SearchJob> SearchJobs { get; set; }
         DbSet<SearchResult> SearchResults { get; set; }
         DbSet<Result> Results { get; set; }
    }
}
