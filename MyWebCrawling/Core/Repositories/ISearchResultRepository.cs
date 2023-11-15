using MyWebCrawling.Core.Models;

namespace MyWebCrawling.Core.Repositories
{
    public interface ISearchResultRepository
    {
        SearchResult SearchedResult(string orginalLink, string parentLink);
        IEnumerable<SearchResult> GetAllSearchResults();
        IEnumerable<SearchResult> GetAllSearchResultsByUrlAddress(string parentLink);
        void Add(SearchResult searchResult);
    }
}
