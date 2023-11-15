using MyWebCrawling.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWebCrawling.Core;
using MyWebCrawling.Persistence.Repositories;

namespace MyWebCrawling.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        public ISearchJobRepository SearchJobs { get; private set; }
        public ISearchResultRepository SearchResults { get; private set; }
        public IResultsRepository Results { get; private set; } 

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            SearchJobs = new SearchJobRepository(context);
            SearchResults = new SearchResultRepository(context);
            Results = new ResultRepository(context);
        }

        public void Complete()
        {
            _context.SaveChanges();
        }
    }

}
