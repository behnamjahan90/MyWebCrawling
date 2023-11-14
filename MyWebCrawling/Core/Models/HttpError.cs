using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MyWebCrawling.Core.Models
{
    public class HttpError
    {
        /// <summary>
        /// Gets/sets the Http status code
        /// </summary>
        public HttpStatusCode HttpStatusCode { get; set; }

        /// <summary>
        /// Gets/sets the URL of the web page when the error was encountered
        /// </summary>
        public string Url { get; set; }
    }
}
