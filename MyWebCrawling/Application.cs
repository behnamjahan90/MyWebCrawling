using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using MyWebCrawling.interfaces;

namespace MyWebCrawling
{
    public class Application : IApplication
    {
        private static IServiceProvider _provider;


        public Application(IServiceProvider provider)
        {
            _provider = provider;
           
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

        public async Task RunTask(string[] args)
        {
            try
            {
                DisplayCommandLineArguments(args);
               

                await Parser.Default.ParseArguments<CmdLineArgumentModel>(args).WithParsedAsync(Run);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static async Task Run(CmdLineArgumentModel arg)
        {
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
