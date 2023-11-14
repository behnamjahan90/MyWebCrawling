using MyWebCrawling.Core.Models;
using MyWebCrawling.interfaces;


namespace MyWebCrawling
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