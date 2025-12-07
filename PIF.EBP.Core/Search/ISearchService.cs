using System.Collections.Generic;
using System.Threading.Tasks;

namespace PIF.EBP.Core.Search
{
    public interface ISearchService
    {
        Task<bool> CreateIndexAsync<T>(string indexName) where T : EntityBase;
        Task<bool> DeleteIndexAsync(string indexName);
        Task<List<object>> SearchAsync(string searchParam);
        Task<bool> IndexDocumentAsync<T>(string indexName, T document) where T : EntityBase;
        Task<bool> UpdateDocumentAsync<T>(string indexName, string id, T document) where T : EntityBase;
    }
}
