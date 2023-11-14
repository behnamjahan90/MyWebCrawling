using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using System.Net;
using MyWebCrawling.Extentions;
using MyWebCrawling.interfaces;
using MyWebCrawling.Core.Models;

namespace MyWebCrawling
{
    public class SingleThreadedWebSiteCrawler : IWebSiteCrawler
    {
        public const int RetryAttempts = 2;
        private readonly List<HttpError> _errors;
        private readonly IHtmlParser _htmlParser;
        private readonly HttpClient _httpClient;
        private readonly Queue<SearchJob> _jobQueue;
        private readonly ILogger<SingleThreadedWebSiteCrawler> _logger;
        private readonly SortedDictionary<string, SearchResult> _searchResults;

        public SingleThreadedWebSiteCrawler(
            ILogger<SingleThreadedWebSiteCrawler> logger,
            IHtmlParser htmlParser,
            HttpClient httpClient)
        {
            _logger = logger;
            _htmlParser = htmlParser;
            _httpClient = httpClient;
            _errors = new List<HttpError>();
            
            _searchResults = new SortedDictionary<string, SearchResult>();
            _jobQueue = new Queue<SearchJob>();
        }

        public async Task<List<SearchResult>> Run(string url, int maxPagesToSearch)
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("User-AgentCrl", "Other");
            _jobQueue.Enqueue(new SearchJob(url, 0));

            //An enhanced algorithm should be set here:
            for (int pageCount = 0; pageCount < maxPagesToSearch; pageCount++)
            {
                if (_jobQueue.Count == 0)
                {
                    _logger.LogInformation("No more pages to search");
                    break;
                }
                await DiscoverLinks(url);
                _logger.LogInformation($"----Pages searched={pageCount}, Job Queue={_jobQueue.Count}, results={_searchResults.Count}, Errors={_errors.Count}");
            }

            return _searchResults.Values.ToList();
        }

        private async Task DiscoverLinks(string startingSite)
        {
            var searchJob = _jobQueue.Dequeue();
            string pageContents = await DownloadPage(searchJob);
            if (pageContents == null) return;
            
            _logger.LogInformation($"Downloaded page:{searchJob.Url}, Content is {pageContents.Length} characters long");
            var links = FindLinksWithinHtml(pageContents);
            _logger.LogInformation($"Found {links.Count} hyperlinks in the page {searchJob.Url}");
            links.ForEach(rawLink =>
            {
                _logger.LogInformation($"Found a link '{rawLink}'");
                var link = rawLink.Trim().Trim('\n');
                _logger.LogInformation($"After trimming link becomes '{link}'");

                //search in db 
                var searchResult = new SearchResult
                {
                    ParentPageUrl = searchJob.Url,
                    OriginalLink = link,
                    Level = searchJob.Level + 1
                };
                bool isLinkAcceptable = IsLinkAcceptable(searchJob, searchResult);
                if (!isLinkAcceptable)
                {
                    _logger.LogInformation($"Ignoring link {link}");
                    return;
                }

                if (link.StartsWith("/"))
                {
                    searchResult.AbsoluteLink = UrlExtensions.Combine(startingSite, link);
                    _logger.LogInformation($"Found a child link:'{link}' which is relateive to top level page:{searchResult.AbsoluteLink} under root:{startingSite}");
                }
                else
                {
                    if (searchResult.IsLinkFullyQualified)
                    {
                        searchResult.AbsoluteLink = new Uri(searchResult.OriginalLink);
                    }
                    else
                    {
                        var parentLink = searchJob.Uri.GetParentUriString();
                        var absoluteUri = UrlExtensions.Combine(parentLink, link);
                        _logger.LogInformation($"Found a child link:'{link}' which is relative to container page:{absoluteUri} under parent:{parentLink}");
                        searchResult.AbsoluteLink = absoluteUri;
                    }
                }

                if (
                (searchResult.AbsoluteLink.Host.ToLower() == searchJob.Uri.Host.ToLower()) &&
                (searchResult.AbsoluteLink.PathAndQuery.ToLower() == searchJob.Uri.PathAndQuery.ToLower())
                )
                {
                    _logger.LogInformation($"Not adding child link:{searchResult.AbsoluteLink.ToString()} because it is the same as parent page");
                    return;
                }
                if (_searchResults.ContainsKey(searchResult.AbsoluteLink.ToString()))
                {
                    _logger.LogInformation($"Child link:{searchResult.AbsoluteLink.ToString()} already added to results");
                    return;
                }
                _searchResults.Add(searchResult.AbsoluteLink.ToString(), searchResult);
                _jobQueue.Enqueue(new SearchJob(searchResult.AbsoluteLink.ToString(), searchResult.Level));
                _logger.LogInformation($"Child link:{searchResult.AbsoluteLink} was added to results");
                _logger.LogInformation($"Queue={_jobQueue.Count} Search results={_searchResults.Count}");
            });
        }
        private List<string> FindLinksWithinHtml(string htmlContent)
        {
            return _htmlParser.GetLinks(htmlContent);
        }

        private async Task<string> DownloadPage(SearchJob searchJob)
        {
            if ((searchJob.Uri.Scheme.ToLower() != "http") 
                && (searchJob.Uri.Scheme.ToLower() != "https")) 
                return null;
            

            var retryPolicy = CreateExponentialBackoffPolicy();

            var htmlResponse = await retryPolicy
                .ExecuteAsync(() => _httpClient.GetAsync(searchJob.Url));

            if (!htmlResponse.IsSuccessStatusCode)
            {
                //throw new NotImplementedException("How do we handle errors? Think"); //TODO handle non-sucess response, Polly retry
                _logger.LogError($"Error while downloading page {searchJob}");
                _errors.Add(new HttpError { Url = searchJob.Url, HttpStatusCode = htmlResponse.StatusCode });
                return null;
            }

            var cType = htmlResponse.Content.Headers.ContentType;
            if (!cType.MediaType.Contains("text/html"))
            {
                _logger.LogInformation($"Content in url:{searchJob} has content type:{cType}. This is non-html Ignoring!");
                return null;
            }

            var htmlContent = await htmlResponse.Content.ReadAsStringAsync();
            return htmlContent;
        }

        private static AsyncRetryPolicy<HttpResponseMessage> CreateExponentialBackoffPolicy()
        {
            var unAcceptableResponses = new []
            {
                HttpStatusCode.GatewayTimeout,
                HttpStatusCode.BadGateway
            };

            return Policy
                .HandleResult<HttpResponseMessage>(resp => unAcceptableResponses.Contains(resp.StatusCode))
                .WaitAndRetryAsync(
                    RetryAttempts,
                    attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));
        }



        private bool IsLinkAcceptable(SearchJob searchJob, SearchResult searchResult)
        {
            var childLink = searchResult.OriginalLink;
            if (string.IsNullOrEmpty(childLink))
            {
                return false;
            }
            var frags = childLink.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (!frags.Any())
            {
                return false;
            }

            if (childLink.StartsWith("#"))
            {
                //Book marks are not wanted
                return false;
            }

            if (childLink.StartsWith("mailto:"))
            {
                //Email links are not wanted
                return false;
            }

            if (childLink.StartsWith("tel:"))
            {
                //Phone links are not wanted
                return false;
            }

            if (childLink.StartsWith("sms:"))
            {
                //sms links are not wanted
                return false;
            }

            if (childLink.ToLower().StartsWith("http:") || childLink.ToLower().StartsWith("https:"))
            {
                searchResult.IsLinkFullyQualified = true;
                var uri = new Uri(childLink);
                if (uri.Host != searchJob.Uri.Host)
                {
                    searchResult.IsLinkExternalDomain = true;
                    return false;
                }
                searchResult.IsLinkExternalDomain = false;
            }

            return true;
        }
    }
}