using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using MyWebCrawling.Core.Factories.Interfaces;
using MyWebCrawling.Core.Factories.Models;
using MyWebCrawling.Core.Models;

namespace MyWebCrawling.Core
{
    public class Application : IApplication
    {
        private static IServiceProvider _provider;
        private readonly IUnitOfWork _unitOfWork;
        public Application(IServiceProvider provider, IUnitOfWork unitOfWork)
        {
            _provider = provider;
            _unitOfWork = unitOfWork;
        }

        private static void DisplayCommandLineArguments(string[] args)
        {
        
            Console.WriteLine("Please Write The website full address:");
            Console.WriteLine("--------------");
            string urlAddress = Console.ReadLine();
         
            args[3] = urlAddress;

            for (int index = 0; index < args.Length; index++)
            {
                Console.WriteLine($"{index}\t\t\t{args[index]}");
            }

            Console.WriteLine("--------------");
        }

        public void WriteWebsiteHistory(Result result)
        {
            Console.WriteLine(
                $"The number of links in the Url:'{result.UrlAddress}' " +
                $"are '{result.ResultCount}' after searching a maximum of '{result.PageCounts}'");
        }

        public async Task RunTask(string[] args)
        {
            try
            {
                Console.WriteLine("Do you want to See your last Try? Y/N");
                string answer = Console.ReadLine();
                if (answer == "y")
                {
                    var results = _unitOfWork.Results.GetAllResults();
                    if (results != null && results.Count() > 0)
                    {
                        Console.WriteLine("Searched Websites");
                        Console.WriteLine(">>>>>>>>>>>>>>>>>>");

                        foreach (var result in _unitOfWork.Results.GetAllResults())
                        {
                            WriteWebsiteHistory(result);
                            
                            Console.WriteLine("********** Related Links Are: **********");
                            foreach (var searchedResult in _unitOfWork.SearchResults.GetAllSearchResultsByUrlAddress(result.UrlAddress))
                            {
                                Console.WriteLine($"{searchedResult.OriginalLink}");
                            }
                            Console.WriteLine("********** Related Links Are: **********");

                            WriteWebsiteHistory(result);
                            Console.WriteLine("--------------");

                        }
                    }
                }

                DisplayCommandLineArguments(args);
                await Parser.Default.ParseArguments<CmdLineArgument>(args).WithParsedAsync(Run);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static async Task Run(CmdLineArgument arg)
        {
            if (string.IsNullOrEmpty(arg.Url))
            {
                Console.WriteLine("the address could not be null or empty");
                return;
            }
            var crawler = _provider.GetService<IWebSiteCrawler>();
            var results = await crawler.Run(arg.Url, arg.MaxSites);
            var formatter = _provider.GetService<ICrawlerResultsFormatter>();
            var stdOut = Console.OpenStandardOutput();
            stdOut.Flush();
            formatter.WriteResults(stdOut, results);
            Console.WriteLine(
                $"Found {results.Count} sites in the Url:'{arg.Url}', after searching a maximum of {arg.MaxSites} sites");
        }
    }

}
