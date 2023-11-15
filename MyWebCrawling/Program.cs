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
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(".\\Properties\\launchSettings.json")
                .Build();

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("MyCrawlerDBLocalConnection"));
            });
            
            services.AddScoped<IApplication, Application>();
            services.AddTransient<IWebSiteCrawler, SingleThreadedWebSiteCrawler>();
            services.AddTransient<IHtmlParser, HtmlAgilityParser>();
            services.AddTransient<HttpClient>();
            services.AddLogging(builder => builder.AddLog4Net("log4net.config"));//ILogger Interface
            services.AddTransient<ICrawlerResultsFormatter, CsvResultsFormatter>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            //services.AddScoped<IGenericRepository<MyModel>, GenericRepository<MyModel>>();
            //services.AddScoped<IMyDbContext, MyDbContext>();

        })
        .Build();

var app =  host.Services.GetRequiredService<IApplication>();

await app.RunTask(args);