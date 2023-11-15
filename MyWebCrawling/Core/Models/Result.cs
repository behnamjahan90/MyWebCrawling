
namespace MyWebCrawling.Core.Models
{
    public class Result
    {
        public int Id { get; set; }

        public int PageCounts { get; set; }

        public int ErrorCounts { get; set; }

        public int JobQueue { get; set; }

        public int ResultCount { get; set; }

        public string UrlAddress { get; set; }
    }
}
