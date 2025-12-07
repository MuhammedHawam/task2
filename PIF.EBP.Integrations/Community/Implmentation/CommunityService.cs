using PIF.EBP.Core.Community;
using PIF.EBP.Core.Community.DTOs;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace PIF.EBP.Integrations.Community.Implmentation
{
    public class CommunityService : ApiClient, ICommunityService
    {
        public CommunityService(): base() { }

        // -------------------------------------------------------
        // Get community details (public)
        // -------------------------------------------------------
        public Task<object> GetCommunityAsync(long communityId) =>
            GetAsync<object>($"communities/{communityId}");

        // -------------------------------------------------------
        // Get all posts for a community (public)
        // -------------------------------------------------------
        public Task<object> GetCommunityPostsAsync(long communityId,
                                                          int page = 1,
                                                          int pageSize = 20) =>
            GetAsync<object>($"communities/{communityId}/posts?page={page}&pageSize={pageSize}");

        // -------------------------------------------------------
        // Get members of a community (public)
        // -------------------------------------------------------
        public Task<object> GetCommunityMembersAsync(long communityId,
                                                            int page = 1,
                                                            int pageSize = 20) =>
            GetAsync<object>($"communities/{communityId}/members?page={page}&pageSize={pageSize}");

        // -------------------------------------------------------
        // Unfollow a community (public)
        // -------------------------------------------------------
        public Task UnfollowCommunityAsync(long communityId) =>
            PostAsync<object>($"communities/{communityId}/unfollow"); // object -> we ignore the body

        // -------------------------------------------------------
        // Suggestions (max 4)
        // -------------------------------------------------------
        public Task<object> GetSuggestedCommunitiesAsync(string search) =>
            GetAsync<object>($"communities/suggestions?search={WebUtility.UrlEncode(search)}");

        // -------------------------------------------------------
        // Paginated list with filters
        // -------------------------------------------------------
        public Task<object> GetCommunitiesAsync(int page = 1,
                                                            int pageSize = 20,
                                                            bool? followedOnly = null,
                                                            bool? publishedOnly = null,
                                                            string filter = null,
                                                            string sort = null,
                                                            string search = null)
        {
            var qs = new List<string>
            {
                $"page={page}",
                $"pageSize={pageSize}",
                $"followedOnly={followedOnly?.ToString().ToLower() ?? "false"}",
                $"publishedOnly={publishedOnly?.ToString().ToLower() ?? "false"}"
            };
            if (!string.IsNullOrWhiteSpace(filter)) qs.Add($"filter={WebUtility.UrlEncode(filter)}");
            if (!string.IsNullOrWhiteSpace(sort)) qs.Add($"sort={WebUtility.UrlEncode(sort)}");
            if (!string.IsNullOrWhiteSpace(search)) qs.Add($"search={WebUtility.UrlEncode(search)}");

            var query = "?" + string.Join("&", qs);
            return GetAsync<object>($"communities{query}");
        }
    }
}
