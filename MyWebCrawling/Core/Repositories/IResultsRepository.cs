using MyWebCrawling.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWebCrawling.Core.Repositories
{
    public interface IResultsRepository
    {
        void Add(Result result);
        IEnumerable<Result> GetAllResults();
        Result GetResult(string url);
    }
}
