
using MyWebCrawling.Core.Repositories;

namespace MyWebCrawling.Core
{
    public interface IUnitOfWork
    {
        ISearchJobRepository SearchJobs { get; }
        ISearchResultRepository SearchResults { get; }
        IResultsRepository Results { get; }
        void Complete();
    }
}
