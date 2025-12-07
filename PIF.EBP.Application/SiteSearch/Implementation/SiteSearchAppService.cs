using PIF.EBP.Core.Search;
using PIF.EBP.Core.Search.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PIF.EBP.Application.SiteSearch.Implementation
{
    public class SiteSearchAppService : ISiteSearchAppService
    {
        private readonly ISearchService _searchService;

        public SiteSearchAppService()
        {
        }

        public async Task<List<object>> SearchAsync(string searchParam)
        {
            return await _searchService.SearchAsync(searchParam);
        }

        public async Task<bool> CreateIndex(string indexName)
        {
            switch (indexName)
            {
                case "request":
                    return await _searchService.CreateIndexAsync<RequestEntity>(indexName);
                case "contact":
                    return await _searchService.CreateIndexAsync<ContactEntity>(indexName);
                case "requeststep":
                    return await _searchService.CreateIndexAsync<RequestStepEntity>(indexName);
                case "calendar":
                    return await _searchService.CreateIndexAsync<CalendarEntity>(indexName);
                default:
                    return false;
            }
        }

        public async Task<bool> DeleteIndex(string indexName)
        {
            return await _searchService.DeleteIndexAsync(indexName);
        }

        public async Task<bool> UpdateDocumentFromCrmToSearchEngine(string indexName, string documentId)
        {
            return await _searchService.UpdateDocumentAsync<RequestEntity>(indexName, documentId, new RequestEntity());
        }
    }
}
