using PIF.EBP.Core.Community.DTOs;
using PIF.EBP.Core.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIF.EBP.Core.Community
{
    public interface ISearchService : ITransientDependency
    {
        /// <summary>
        /// Global search that returns both communities and posts.
        /// </summary>
        Task<object> GlobalSearchAsync(string search,
                                                int page = 1,
                                                int pageSize = 20);
    }
}
