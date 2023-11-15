
namespace MyWebCrawling.Core.Models
{
    public class SearchJob
    {
        public int Id { get; set; }

        public SearchJob()
        {
   
        }

        public int Level { get;  set; }

        public Uri Uri { get; set; }

        public string Url { get;  set; }

        public override string ToString()
        {
            return $"Level={this.Level} Url={this.Url}";
        }
    }
}
