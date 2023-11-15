using MyWebCrawling.Core.Models;
using MyWebCrawling.Core.Repositories;

namespace MyWebCrawling.Persistence.Repositories
{
    public class SearchJobRepository : ISearchJobRepository
    {
        private readonly IApplicationDbContext _context;

        public SearchJobRepository(IApplicationDbContext context)
        {
            _context = context;
        }

        public SearchJob SearchedJob(string url)
        {
            return _context.SearchJobs
                .SingleOrDefault(s => s.Url == url);
        }

        public IEnumerable<SearchJob> GetAllSearchJobs()
        {
            return _context.SearchJobs.ToList();
        }

        public void Add(SearchJob searchJob)
        {
            _context.SearchJobs.Add(searchJob);
        }


    }
}
