using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyWebCrawling;
using MyWebCrawling.interfaces;
using MyWebCrawling.Persistence;
using System.Data.Entity;



IHost host = Host.CreateDefaultBuilder().ConfigureServices(
        services =>
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(".\\Properties\\launchSettings.json")
                .Build();

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("MyCrawlerDBLocalConnection"));
            });

            services.AddSingleton<IApplication, Application>();
            services.AddTransient<IWebSiteCrawler, SingleThreadedWebSiteCrawler>();
            services.AddTransient<IHtmlParser, HtmlAgilityParser>();
            services.AddTransient<HttpClient>();
            services.AddLogging(builder => builder.AddLog4Net("log4net.config"));//ILogger Interface
            services.AddTransient<ICrawlerResultsFormatter, CsvResultsFormatter>();
           
            //services.AddScoped<IGenericRepository<MyModel>, GenericRepository<MyModel>>();
            //services.AddScoped<IMyDbContext, MyDbContext>();

        })
        .Build();

var app =  host.Services.GetRequiredService<IApplication>();

await app.RunTask(args);