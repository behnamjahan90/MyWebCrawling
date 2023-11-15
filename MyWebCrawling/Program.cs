using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyWebCrawling.Core;
using MyWebCrawling.Persistence;
using MyWebCrawling.Core.Factories.Actions;
using MyWebCrawling.Core.Factories.Interfaces;


IHost host = Host.CreateDefaultBuilder().ConfigureServices(
        services =>
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("Properties/launchSettings.json", optional: true, reloadOnChange: true)
                .Build();

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            });
            
            services.AddScoped<IApplication, Application>();
            services.AddTransient<IWebSiteCrawler, SingleThreadedWebSiteCrawler>();
            services.AddTransient<IHtmlParser, HtmlAgilityParser>();
            services.AddTransient<HttpClient>();
            services.AddLogging(builder => builder.AddLog4Net("log4net.config")
                .AddFilter("Microsoft.EntityFrameworkCore.Database.Command",
                    LogLevel.Warning));//ILogger Interface

            services.AddTransient<ICrawlerResultsFormatter, CsvResultsFormatter>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

        })
        .Build();

var app =  host.Services.GetRequiredService<IApplication>();

await app.RunTask(args);