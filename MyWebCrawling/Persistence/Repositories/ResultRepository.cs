using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using MyWebCrawling.Core.Models;
using MyWebCrawling.Core.Repositories;

namespace MyWebCrawling.Persistence.Repositories
{
    public class ResultRepository : IResultsRepository
    {
        private readonly IApplicationDbContext _context;

        public ResultRepository(IApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Result> GetAllResults()
        {
            return _context.Results.ToList();
        }

        public Result GetResult(string url)
        {
            return _context.Results
                .SingleOrDefault(r => r.UrlAddress == url);
        }

        public void Add(Result result)
        {
            _context.Results.Add(result);
        }
    }
}
