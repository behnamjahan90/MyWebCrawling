using System.Net;
using System.Security.Policy;
using Microsoft.Extensions.Logging;
using MyWebCrawling.Core.Factories.Interfaces;
using MyWebCrawling.Core.Factories.Models;
using MyWebCrawling.Core.Models;
using MyWebCrawling.Extentions;
using MyWebCrawling.Persistence;
using Polly;
using Polly.Retry;

namespace MyWebCrawling.Core.Factories.Actions
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
        private readonly IUnitOfWork _unitOfWork;

        public SingleThreadedWebSiteCrawler(
            ILogger<SingleThreadedWebSiteCrawler> logger,
            IHtmlParser htmlParser,
            HttpClient httpClient,
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _htmlParser = htmlParser;
            _httpClient = httpClient;
            _errors = new List<HttpError>();
            _unitOfWork = unitOfWork;

            _searchResults = new SortedDictionary<string, SearchResult>();
            _jobQueue = new Queue<SearchJob>();
        }

        public async Task<List<SearchResult>> Run(string url, int maxPagesToSearch)
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("CrawlerUser", "Test");

            var searchJob = _unitOfWork.SearchJobs.SearchedJob(url);

            if (searchJob==null)
            {
                searchJob = new SearchJob
                {
                    Url = url,
                    Uri = new Uri(url),
                    Level = 0
                };
                _unitOfWork.SearchJobs.Add(searchJob);
                _unitOfWork.Complete();
            }
            
            _jobQueue.Enqueue(searchJob);

            var result = _unitOfWork.Results.GetResult(url);
            int jobQueCount = 0;
            int pageCounts = 0;
            int resultCounts = 0;
            int errorCounts = 0;

            //An enhanced algorithm should be set here:
            for (int pageCount = 0; pageCount < maxPagesToSearch; pageCount++)
            {
                if (_jobQueue.Count == 0)
                {
                    _logger.LogInformation("No more pages to search");
                    break;
                }
                await DiscoverLinks(url);

                pageCounts += pageCount;
                jobQueCount += _jobQueue.Count;
                resultCounts += _searchResults.Count;
                errorCounts += _errors.Count;

                _logger.LogInformation($"----Pages searched={pageCount}, Job Queue={_jobQueue.Count}, results={_searchResults.Count}, Errors={_errors.Count}");
            }

            if (result == null)
            {
                result = new Result
                {
                    UrlAddress = url,
                    PageCounts = pageCounts,
                    JobQueue = jobQueCount,
                    ResultCount= resultCounts,
                    ErrorCounts = errorCounts
                };
                _unitOfWork.Results.Add(result);
            }
            else
            {
                result.UrlAddress = url;
                result.PageCounts = pageCounts;
                result.JobQueue = jobQueCount;
                result.ResultCount = resultCounts;
                result.ErrorCounts = errorCounts;
            }
            _unitOfWork.Complete();

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
                var searchResult = _unitOfWork.SearchResults.SearchedResult(link, searchJob.Url);
                bool isnewResult = false;
                if (searchResult == null)
                {
                    searchResult = new SearchResult
                    {
                        ParentPageUrl = searchJob.Url,
                        OriginalLink = link,
                        Level = searchJob.Level + 1
                    };
                    isnewResult = true;
                }
                else
                {
                    searchResult.ParentPageUrl = searchJob.Url;
                    searchResult.OriginalLink = link;
                    searchResult.Level = searchJob.Level + 1;
                }

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

                if (isnewResult)
                {
                    _unitOfWork.SearchResults.Add(searchResult);
                }
                
      
                _searchResults.Add(searchResult.AbsoluteLink.ToString(), searchResult);
                var searchedJob = _unitOfWork.SearchJobs.SearchedJob(searchResult.AbsoluteLink.ToString());
                if (searchedJob == null)
                {
                    searchedJob = new SearchJob
                    {
                        Url = searchResult.AbsoluteLink.ToString(),
                        Level = searchResult.Level,
                        Uri = new Uri(searchResult.AbsoluteLink.ToString())
                    };

                    _unitOfWork.SearchJobs.Add(searchJob);
                }
                else
                {
                    searchedJob.Url = searchResult.AbsoluteLink.ToString();
                    searchedJob.Level = searchResult.Level;
                    searchedJob.Uri = new Uri(searchResult.AbsoluteLink.ToString());
                }
                _unitOfWork.Complete();

                _jobQueue.Enqueue(searchedJob);
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