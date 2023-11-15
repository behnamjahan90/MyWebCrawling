using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWebCrawling.Core.Models
{
    public class SearchResult
    {
        public int Id { get; set; }

        public Uri AbsoluteLink { get; set; }

        public bool IsLinkExternalDomain { get; set; }

        public bool IsLinkFullyQualified { get; set; }

        public int Level { get; set; }

        public string OriginalLink { get; set; }

        public string ParentPageUrl { get; set; }

        public override string ToString()
        {
            return $"Parent={this.ParentPageUrl}    Child={this.AbsoluteLink}   Level={this.Level}, IsLinkFullyQualified={this.IsLinkFullyQualified}";
        }
    }
}
