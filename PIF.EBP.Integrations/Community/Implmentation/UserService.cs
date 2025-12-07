using PIF.EBP.Core.Community;
using PIF.EBP.Core.Community.DTOs;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace PIF.EBP.Integrations.Community.Implmentation
{
    public class UserService : ApiClient, IUserService
    {
        public UserService(): base() { }

        // -------------------------------------------------------
        // 1️⃣ Community actions (follow / suggest)
        // -------------------------------------------------------
        public Task<object> FollowCommunityAsync(long communityId) =>
            PostAsync<object>($"user/communities/{communityId}/follow");

        public Task<object> SuggestCommunityAsync(CommunityCreateRequest request) =>
            PostAsync<object>("user/communities/suggest", request);

        // -------------------------------------------------------
        // 2️⃣ Posts (CRUD + like / unlike)
        // -------------------------------------------------------
        public Task<object> CreatePostAsync(PostCreateRequest request) =>
            PostAsync<object>("user/posts", request);

        public Task<object> GetPostAsync(long postId) =>
            GetAsync<object>($"user/posts/{postId}");

        public Task<object> GetMyPostsAsync(int page = 1, int pageSize = 20,
                                                   string filter = null, string sort = null,
                                                   string search = null)
        {
            var qs = BuildQuery(page, pageSize, filter, sort, search);
            return GetAsync<object>($"user/posts{qs}");
        }

        public Task<object> UpdatePostAsync(long postId, PostUpdateRequest request) =>
            PutAsync<object>($"user/posts/{postId}", request);

        public Task DeletePostAsync(long postId) =>
            DeleteAsync($"user/posts/{postId}");

        public Task LikePostAsync(long postId) =>
            PostAsync<object>($"user/posts/{postId}/like");

        public Task UnlikePostAsync(long postId) =>
            PostAsync<object>($"user/posts/{postId}/unlike");

        // -------------------------------------------------------
        // 3️⃣ Comments (CRUD)
        // -------------------------------------------------------
        public Task<object> AddCommentAsync(long postId, CommentCreateRequest request) =>
            PostAsync<object>($"user/posts/{postId}/comments", request);

        public Task<object> GetCommentsAsync(long postId, int page = 1, int pageSize = 20) =>
            GetAsync<object>($"user/posts/{postId}/comments?page={page}&pageSize={pageSize}");

        public Task<object> UpdateCommentAsync(long commentId, CommentCreateRequest request) =>
            PutAsync<object>($"user/comments/{commentId}", request);

        public Task DeleteCommentAsync(long commentId) =>
            DeleteAsync($"user/comments/{commentId}");

        // -------------------------------------------------------
        // 4️⃣ Feed (posts from followed communities)
        // -------------------------------------------------------
        public Task<object> GetFeedAsync(int page = 1, int pageSize = 20) =>
            GetAsync<object>($"user/feed?page={page}&pageSize={pageSize}");

        // -------------------------------------------------------
        // 5️⃣ KPI (user specific)
        // -------------------------------------------------------
        public Task<object> GetKpiAsync() => GetAsync<object>("user/kpi");

        // -------------------------------------------------------
        // Helper – same as AdminService (kept private to this class)
        // -------------------------------------------------------
        private static string BuildQuery(int page, int pageSize,
                                         string filter, string sort, string search)
        {
            var q = new List<string>
            {
                $"page={page}",
                $"pageSize={pageSize}"
            };
            if (!string.IsNullOrWhiteSpace(filter)) q.Add($"filter={WebUtility.UrlEncode(filter)}");
            if (!string.IsNullOrWhiteSpace(sort)) q.Add($"sort={WebUtility.UrlEncode(sort)}");
            if (!string.IsNullOrWhiteSpace(search)) q.Add($"search={WebUtility.UrlEncode(search)}");
            return "?" + string.Join("&", q);
        }

        public Task<object> GetProfileMemberAsync(string userId) =>
           GetAsync<object>($"/user/member-profile?userId ={userId}");
    }
}
