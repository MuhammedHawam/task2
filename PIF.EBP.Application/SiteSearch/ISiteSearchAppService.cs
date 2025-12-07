using PIF.EBP.Core.DependencyInjection;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PIF.EBP.Application.SiteSearch
{
    public interface ISiteSearchAppService : ITransientDependency
    {
        Task<List<object>> SearchAsync(string searchParam);
        Task<bool> CreateIndex(string indexName);
        Task<bool> DeleteIndex(string indexName);
        Task<bool> UpdateDocumentFromCrmToSearchEngine(string indexName, string documentId);
    }
}
