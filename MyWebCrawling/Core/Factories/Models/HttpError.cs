using System.Net;

namespace MyWebCrawling.Core.Factories.Models
{
    public class HttpError
    {
        public HttpStatusCode HttpStatusCode { get; set; }
        public string Url { get; set; }
    }
}
