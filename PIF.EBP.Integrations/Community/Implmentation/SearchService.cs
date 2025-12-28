using PIF.EBP.Core.Community;
using PIF.EBP.Core.Community.DTOs;
using System.Net;
using System.Threading.Tasks;

namespace PIF.EBP.Integrations.Community.Implmentation
{
    public class SearchService : CommunityApiClient, ISearchService
    {
        public SearchService(): base() { }

        public Task<object> GlobalSearchAsync(string search,
                                                        int page = 1,
                                                        int pageSize = 20)
        {
            var qs = $"?search={WebUtility.UrlEncode(search)}&page={page}&pageSize={pageSize}";
            return GetAsync<object>($"search{qs}");
        }
    }
}
