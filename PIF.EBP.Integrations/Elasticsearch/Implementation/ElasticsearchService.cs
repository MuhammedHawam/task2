using Elasticsearch.Net;
using Nest;
using PIF.EBP.Core;
using PIF.EBP.Core.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace PIF.EBP.Integrations.Elasticsearch.Implementation
{
    public class ElasticsearchService : ISearchService
    {
        private readonly IElasticClient _client;
        public ElasticsearchService(Uri elasticsearchUri)
        {
            var settings = new ConnectionSettings(elasticsearchUri)
                .BasicAuthentication("elastic", new NetworkCredential(string.Empty, "P@ssw0rd").SecurePassword);
            _client = new ElasticClient(settings);
        }

        public async Task<bool> CreateIndexAsync<T>(string indexName) where T : EntityBase
        {
            var createIndexResponse = await _client.Indices.CreateAsync(indexName, c => c
                .Map<T>(m => m.AutoMap())
            );

            return createIndexResponse.IsValid;
        }

        public async Task<bool> DeleteIndexAsync(string indexName)
        {
            var response = await _client.Indices.DeleteAsync(indexName);

            return response.IsValid;
        }

        public async Task<List<object>> SearchAsync(string searchParam)
        {
            var searchResponse = await _client.SearchAsync<object>(s => s
                .Index("*") // Search across all indices
                .Query(q => q
                    .QueryString(qs => qs
                        .Query(searchParam)
                        .DefaultField("_all") // Search in all fields
                    )
                )
            );

            var results = searchResponse.Documents
                .Cast<object>()
                .ToList();

            return results;
        }

        public async Task<bool> IndexDocumentAsync<T>(string indexName, T document) where T : EntityBase
        {
            var indexResponse = await _client.IndexAsync(document, idx => idx
            .Index(indexName)
            .Refresh(Refresh.WaitFor));

            return indexResponse.IsValid;
        }

        public async Task<bool> UpdateDocumentAsync<T>(string indexName, string id, T document) where T : EntityBase
        {
            var updateResponse = await _client.UpdateAsync<T, object>(id, u => u
                .Index(indexName)
                .Doc(document)
                .RetryOnConflict(3) // Optional: Specify how many times to retry on version conflict
                .Refresh(Refresh.WaitFor)
            );

            return updateResponse.IsValid;
        }
    }
}
