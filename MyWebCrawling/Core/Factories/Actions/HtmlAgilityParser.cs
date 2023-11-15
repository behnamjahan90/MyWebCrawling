using HtmlAgilityPack;
using MyWebCrawling.Core.Factories.Interfaces;

namespace MyWebCrawling.Core.Factories.Actions
{
    public class HtmlAgilityParser : IHtmlParser
    {
        public List<string> GetLinks(string htmlContent)
        {
            if (string.IsNullOrWhiteSpace(htmlContent))
            {
                return new List<string>();
            }
            var document = new HtmlDocument();
            document.LoadHtml(htmlContent);
            var linkNodes = document.DocumentNode.SelectNodes("//a[@href]");
            if (linkNodes == null)
            {
                return new List<string>();
            }
            var links = linkNodes.Where(n => n.Attributes.Contains("href")).Select(n => n.Attributes["href"]).ToList();
            //return links.Where(l=>l.hr)
            return links.Select(l => l.Value).ToList();
        }
    }
}