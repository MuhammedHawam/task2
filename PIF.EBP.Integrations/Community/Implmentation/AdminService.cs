using PIF.EBP.Core.Community;
using PIF.EBP.Core.Community.DTOs;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace PIF.EBP.Integrations.Community.Implmentation
{
    public class AdminService : ApiClient, IAdminService
    {
        public AdminService(): base() { }

        // -------------------------------------------------------
        // Communities (admin)
        // -------------------------------------------------------
        public Task<object> GetCommunityAsync(long communityId) =>
            GetAsync<object>($"admin/communities/{communityId}");

        public Task<object> CreateCommunityAsync(CommunityCreateRequest request) =>
            PostAsync<object>("admin/communities", request);

        public Task<object> UpdateCommunityAsync(long communityId, CommunityUpdateRequest request) =>
            PutAsync<object>($"admin/communities/{communityId}", request);

        public Task DeleteCommunityAsync(long communityId) =>
            DeleteAsync($"admin/communities/{communityId}");

        public Task<object> ApproveCommunityAsync(long communityId) =>
            PutAsync<object>($"admin/communities/{communityId}/approve");

        public Task<object> ArchiveCommunityAsync(long communityId) =>
            PutAsync<object>($"admin/communities/{communityId}/archive");

        public Task<object> UnArchiveCommunityAsync(long communityId) =>
            PutAsync<object>($"admin/communities/{communityId}/unarchive");

        public Task<object> RejectCommunityAsync(long communityId, CommunityRejectRequest request) =>
            PutAsync<object>($"admin/communities/{communityId}/reject", request);

        public Task<object> PublishCommunityAsync(long communityId, CommunityPublishRequest request) =>
            PutAsync<object>($"admin/communities/{communityId}/publish", request);

        public Task<object> GetAllCommunitiesAsync(int page = 1, int pageSize = 20) =>
            GetAsync<object>($"admin/communities?page={page}&pageSize={pageSize}");

        public Task<object> GetPendingCommunitiesAsync(int page = 1, int pageSize = 20,
                                                                   string filter = null, string sort = null,
                                                                   string search = null)
        {
            var qs = BuildQuery(page, pageSize, filter, sort, search);
            return GetAsync<object>($"admin/communities/pending{qs}");
        }
        public Task<object> GetCommunityFollowersAsync(long communityId, long userId) =>
            GetAsync<object>($"admin/communities/{communityId}/followers/{userId}");

        public Task<object> GetApprovedCommunitiesReadyToPublishAsync(int page = 1, int pageSize = 20,
                                                                 string filter = null, string sort = null,
                                                                 string search = null, string status = null)
        {
            var qs = BuildQuery(page, pageSize, filter, sort, search, status);
            return GetAsync<object>($"admin/communities/my-tasks{qs}");
        }
        public Task<object> GetAdminKpiAsync() =>
            GetAsync<object>("admin/kpi");

        // -------------------------------------------------------
        // Posts (admin)
        // -------------------------------------------------------
        public Task<object> GetPendingPostsAsync(int page = 1, int pageSize = 20,
                                                         string filter = null, string sort = null,
                                                         string search = null)
        {
            var qs = BuildQuery(page, pageSize, filter, sort, search);
            return GetAsync<object>($"admin/posts/pending{qs}");
        }

        public Task<object> ArchivePostsAsync(long postId) =>
            PutAsync<object>($"admin/posts/{postId}/archive");

        public Task<object> UnArchivePostsAsync(long postId) =>
            PutAsync<object>($"admin/posts/{postId}/unarchive");


        public Task<object> UpdatePostStatusAsync(long postId, PostStatusUpdateRequest request) =>
            PutAsync<object>($"admin/posts/{postId}/status", request);

        public Task DeletePostAsync(long postId) =>
            DeleteAsync($"admin/posts/{postId}");

        public Task<object> GetPostsTasksDependsOnRoleAsync(int page = 1, int pageSize = 20,
                                                               string filter = null, string sort = null,
                                                               string search = null, string status = null)
        {
            var qs = BuildQuery(page, pageSize, filter, sort, search, status);
            return GetAsync<object>($"admin/posts/my-tasks{qs}");
        }
        // -------------------------------------------------------
        // Comments (admin)
        // -------------------------------------------------------
        public Task DeleteCommentAsync(long commentId) =>
            DeleteAsync($"admin/comments/{commentId}");

        // -------------------------------------------------------
        // Followers (admin)
        // -------------------------------------------------------
        public Task<object> FollowCommunityAsync(long communityId, long userId,
                                                               CommunityFollowerCreateRequest request)
        {
            var url = $"admin/communities/{communityId}/followers/{userId}";
            return PostAsync<object>(url, request);
        }

        // -------------------------------------------------------
        // Helper – builds optional query string parts
        // -------------------------------------------------------
        private static string BuildQuery(int page, int pageSize,
                                         string filter, string sort, string search,string status = null)
        {
            var q = new List<string>
            {
                $"page={page}",
                $"pageSize={pageSize}"
            };
            if (!string.IsNullOrWhiteSpace(filter)) q.Add($"filter={WebUtility.UrlEncode(filter)}");
            if (!string.IsNullOrWhiteSpace(sort)) q.Add($"sort={WebUtility.UrlEncode(sort)}");
            if (!string.IsNullOrWhiteSpace(search)) q.Add($"search={WebUtility.UrlEncode(search)}");
            if (!string.IsNullOrWhiteSpace(status)) q.Add($"status={WebUtility.UrlEncode(status)}");
            return "?" + string.Join("&", q);
        }
    }
}
