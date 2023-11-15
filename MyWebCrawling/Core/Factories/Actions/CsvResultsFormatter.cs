using MyWebCrawling.Core.Factories.Interfaces;
using MyWebCrawling.Core.Models;

namespace MyWebCrawling.Core.Factories.Actions
{
    public class CsvResultsFormatter : ICrawlerResultsFormatter
    {
        public void WriteResults(Stream output, List<SearchResult> searchResults)
        {
            var swriter = new StreamWriter(output);
            swriter.AutoFlush = true;
            swriter.WriteLine($"Url");
            foreach (var result in searchResults)
            {
                swriter.WriteLine($"{result.AbsoluteLink}");
            }
        }
    }
}