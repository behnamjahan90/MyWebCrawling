
using MyWebCrawling.Core.Models;
using MyWebCrawling.Core.Repositories;


namespace MyWebCrawling.Persistence.Repositories
{
    public class SearchResultRepository : ISearchResultRepository
    {
        private readonly IApplicationDbContext _context;
        public SearchResultRepository(IApplicationDbContext context)
        {
            _context = context;
        }

        public SearchResult SearchedResult(string orginalLink,string parentLink)
        {
            return _context.SearchResults
                .SingleOrDefault(s => s.OriginalLink.Trim().ToLower().Equals(orginalLink.Trim().ToLower()) 
                                      && s.ParentPageUrl.Trim().ToLower().Equals(parentLink.Trim().ToLower()));
        }

        public IEnumerable<SearchResult> GetAllSearchResultsByUrlAddress(string parentLink)
        {
            return _context.SearchResults
                .Where(s => s.ParentPageUrl == parentLink).ToList();
        }

        public IEnumerable<SearchResult> GetAllSearchResults()
        {
            return _context.SearchResults.ToList();
        }

        public void Add(SearchResult searchResult)
        {
            _context.SearchResults.Add(searchResult);
        }
    }
}
