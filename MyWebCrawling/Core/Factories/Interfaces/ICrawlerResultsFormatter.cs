using MyWebCrawling.Core.Models;

namespace MyWebCrawling.Core.Factories.Interfaces
{
    /// <summary>
    /// Converts the results of the crawler to the desired output. Example: CSV, JSON
    /// </summary>
    public interface ICrawlerResultsFormatter
    {
        void WriteResults(Stream output, List<SearchResult> searchResults);
    }
}