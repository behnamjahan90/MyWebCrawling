using MyWebCrawling.Core.Models;

namespace MyWebCrawling.Core.Repositories
{
    public interface ISearchJobRepository
    {
        void Add(SearchJob searchJob);
        IEnumerable<SearchJob> GetAllSearchJobs();
        SearchJob SearchedJob(string url);
    }
}
