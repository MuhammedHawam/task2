using PIF.EBP.Core.Community.DTOs;
using PIF.EBP.Core.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIF.EBP.Application.Community
{
    public interface ICommunityManagementService : ITransientDependency
    {
        #region ---- Admin (community) ----
        Task<object> GetCommunityAsync(long communityId);
        Task<object> CreateCommunityAsync(CommunityCreateRequest request);
        Task<object> UpdateCommunityAsync(long communityId, CommunityUpdateRequest request);
        Task DeleteCommunityAsync(long communityId);
        Task<object> ArchiveCommunityAsync(long communityId);
        Task<object> UnArchiveCommunityAsync(long communityId);
        Task<object> ApproveCommunityAsync(long communityId);
        Task<object> RejectCommunityAsync(long communityId, CommunityRejectRequest request);
        Task<object> PublishCommunityAsync(long communityId, CommunityPublishRequest request);
        Task<object> GetAllCommunitiesAsync(int page = 1, int pageSize = 20);
        Task<object> GetPendingCommunitiesAsync(int page = 1,
                                                                         int pageSize = 20,
                                                                         string filter = null,
                                                                         string sort = null,
                                                                         string search = null);
        Task<object> GetAdminKpiAsync();

        Task<object> GetPendingPostsAsync(int page = 1,
                                                                   int pageSize = 20,
                                                                   string filter = null,
                                                                   string sort = null,
                                                                   string search = null);

        Task<object> UnArchivePostsAsync(long communityId);

        Task<object> ArchivePostsAsync(long communityId);

        Task<object> UpdatePostStatusAsync(long postId, PostStatusUpdateRequest request);
        Task DeleteAdminPostAsync(long postId);
        Task DeleteCommentAsync(long commentId);
        Task<object> FollowCommunityAsAdminAsync(long communityId,
                                                                           long userId,
                                                                           CommunityFollowerCreateRequest request);

        Task<object> GetCommunityFollowersAsync(long communityId, long userId);
        Task<object> GetPostsTasksDependsOnRoleAsync(
            int page = 1,
            int pageSize = 20,
            string filter = null,
            string sort = null,
            string search = null, string status = null);
        Task<object> GetApprovedCommunitiesReadyToPublishAsync(
            int page = 1,
            int pageSize = 20,
            string filter = null,
            string sort = null,
            string search = null, string status = null);
        #endregion

        #region ---- Public community (read‑only) ----
        Task<object> GetPublicCommunityAsync(long communityId);
        Task<object> GetPublicCommunityPostsAsync(long communityId,
                                                                          int page = 1,
                                                                          int pageSize = 20);
        Task<object> GetPublicCommunityMembersAsync(long communityId,
                                                                            int page = 1,
                                                                            int pageSize = 20);
        Task UnfollowCommunityAsync(long communityId);
        Task<object> GetSuggestedCommunitiesAsync(string search);
        Task<object> GetCommunitiesAsync(int page = 1,
                                                                 int pageSize = 20,
                                                                 bool? followedOnly = null,
                                                                 bool? publishedOnly = null,
                                                                 string filter = null,
                                                                 string sort = null,
                                                                 string search = null);
        #endregion

        #region ---- User (private) ----
        Task<object> FollowCommunityAsync(long communityId);
        Task<object> SuggestCommunityAsync(CommunityCreateRequest request);

        Task<object> CreatePostAsync(PostCreateRequest request);
        Task<object> GetMyPostAsync(long postId);
        Task<object> GetMyPostsAsync(int page = 1,
                                                               int pageSize = 20,
                                                               string filter = null,
                                                               string sort = null,
                                                               string search = null);
        Task<object> UpdateMyPostAsync(long postId, PostUpdateRequest request);
        Task DeleteMyPostAsync(long postId);
        Task LikePostAsync(long postId);
        Task UnlikePostAsync(long postId);

        Task<object> AddCommentAsync(long postId, CommentCreateRequest request);
        Task<object> GetCommentsAsync(long postId, int page = 1, int pageSize = 20);
        Task<object> UpdateCommentAsync(long commentId, CommentCreateRequest request);
        Task DeleteMyUserCommentAsync(long commentId);
        Task<object> GetFeedAsync(int page = 1, int pageSize = 20);
        Task<object> GetUserKpiAsync();
        #endregion

        #region ---- Global search ----
        Task<object> GlobalSearchAsync(string search,
                                                                int page = 1,
                                                                int pageSize = 20);
        #endregion

        Task<object> GetProfileMemberAsync(string userId, string companyId);
        Task DeleteHistoryById(long historyId);
        Task DeleteAllHistory();
        Task<object> GetSearchHistory();


    }
}
