using PIF.EBP.Core.Community;
using PIF.EBP.Core.Community.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIF.EBP.Application.Community.Implmentation
{
    public class CommunityManagementService : ICommunityManagementService
    {
        private readonly IAdminService _adminService;
        private readonly ICommunityService _publicCommunityService;
        private readonly IUserService _userService;
        private readonly ISearchService _searchService;

        public CommunityManagementService(
            IAdminService adminService,
            ICommunityService publicCommunityService,
            IUserService userService,
            ISearchService searchService)
        {
            _adminService = adminService ?? throw new ArgumentNullException(nameof(adminService));
            _publicCommunityService = publicCommunityService ?? throw new ArgumentNullException(nameof(publicCommunityService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _searchService = searchService ?? throw new ArgumentNullException(nameof(searchService));
        }

        #region ---- Admin (community) ----
        public Task<object> GetCommunityAsync(long communityId) =>
            _adminService.GetCommunityAsync(communityId);

        public Task<object> CreateCommunityAsync(CommunityCreateRequest request) =>
            _adminService.CreateCommunityAsync(request);

        public Task<object> UpdateCommunityAsync(long communityId, CommunityUpdateRequest request) =>
            _adminService.UpdateCommunityAsync(communityId, request);

        public Task DeleteCommunityAsync(long communityId) =>
            _adminService.DeleteCommunityAsync(communityId);

        public Task<object> ArchiveCommunityAsync(long communityId) =>
            _adminService.ArchiveCommunityAsync(communityId);

        public Task<object> UnArchiveCommunityAsync(long communityId) =>
            _adminService.UnArchiveCommunityAsync(communityId);
        public Task<object> ApproveCommunityAsync(long communityId) =>
            _adminService.ApproveCommunityAsync(communityId);

        public Task<object> RejectCommunityAsync(long communityId, CommunityRejectRequest request) =>
            _adminService.RejectCommunityAsync(communityId, request);

        public Task<object> PublishCommunityAsync(long communityId, CommunityPublishRequest request) =>
            _adminService.PublishCommunityAsync(communityId, request);

        public Task<object> GetAllCommunitiesAsync(int page = 1, int pageSize = 20) =>
            _adminService.GetAllCommunitiesAsync(page, pageSize);

        public Task<object> GetPendingCommunitiesAsync(int page = 1,
                                                                                 int pageSize = 20,
                                                                                 string filter = null,
                                                                                 string sort = null,
                                                                                 string search = null) =>
            _adminService.GetPendingCommunitiesAsync(page, pageSize, filter, sort, search);

        public Task<object> GetAdminKpiAsync() =>
            _adminService.GetAdminKpiAsync();

        public Task<object> GetPendingPostsAsync(int page = 1,
                                                                        int pageSize = 20,
                                                                        string filter = null,
                                                                        string sort = null,
                                                                        string search = null) =>
            _adminService.GetPendingPostsAsync(page, pageSize, filter, sort, search);


        public Task<object> UnArchivePostsAsync(long communityId) =>
            _adminService.UnArchivePostsAsync(communityId);
        public Task<object> ArchivePostsAsync(long communityId) =>
            _adminService.ArchivePostsAsync(communityId);

        public Task<object> UpdatePostStatusAsync(long postId, PostStatusUpdateRequest request) =>
            _adminService.UpdatePostStatusAsync(postId, request);

        public Task DeleteAdminPostAsync(long postId) =>
            _adminService.DeletePostAsync(postId);

        public Task DeleteCommentAsync(long commentId) =>
            _adminService.DeleteCommentAsync(commentId);

        public Task<object> FollowCommunityAsAdminAsync(long communityId,
                                                                                 long userId,
                                                                                 CommunityFollowerCreateRequest request) =>
            _adminService.FollowCommunityAsync(communityId, userId, request);
        #endregion

        #region ---- Public community (read‑only) ----
        public Task<object> GetPublicCommunityAsync(long communityId) =>
            _publicCommunityService.GetCommunityAsync(communityId);

        public Task<object> GetPublicCommunityPostsAsync(long communityId,
                                                                                 int page = 1,
                                                                                 int pageSize = 20) =>
            _publicCommunityService.GetCommunityPostsAsync(communityId, page, pageSize);

        public Task<object> GetPublicCommunityMembersAsync(long communityId,
                                                                                   int page = 1,
                                                                                   int pageSize = 20) =>
            _publicCommunityService.GetCommunityMembersAsync(communityId, page, pageSize);

        public Task UnfollowCommunityAsync(long communityId) =>
            _publicCommunityService.UnfollowCommunityAsync(communityId);

        public Task<object> GetSuggestedCommunitiesAsync(string search) =>
            _publicCommunityService.GetSuggestedCommunitiesAsync(search);

        public Task<object> GetCommunitiesAsync(int page = 1,
                                                                         int pageSize = 20,
                                                                         bool? followedOnly = null,
                                                                         bool? publishedOnly = null,
                                                                         string filter = null,
                                                                         string sort = null,
                                                                         string search = null) =>
            _publicCommunityService.GetCommunitiesAsync(page, pageSize, followedOnly,
                                                        publishedOnly, filter, sort, search);
        #endregion

        #region ---- User (private) ----
        public Task<object> FollowCommunityAsync(long communityId) =>
            _userService.FollowCommunityAsync(communityId);

        public Task<object> SuggestCommunityAsync(CommunityCreateRequest request) =>
            _userService.SuggestCommunityAsync(request);

        public Task<object> CreatePostAsync(PostCreateRequest request) =>
            _userService.CreatePostAsync(request);

        public Task<object> GetMyPostAsync(long postId) =>
            _userService.GetPostAsync(postId);

        public Task<object> GetMyPostsAsync(int page = 1,
                                                                   int pageSize = 20,
                                                                   string filter = null,
                                                                   string sort = null,
                                                                   string search = null) =>
            _userService.GetMyPostsAsync(page, pageSize, filter, sort, search);

        public Task<object> UpdateMyPostAsync(long postId, PostUpdateRequest request) =>
            _userService.UpdatePostAsync(postId, request);

        public Task DeleteMyPostAsync(long postId) =>
            _userService.DeletePostAsync(postId);

        public Task LikePostAsync(long postId) =>
            _userService.LikePostAsync(postId);

        public Task UnlikePostAsync(long postId) =>
            _userService.UnlikePostAsync(postId);

        public Task<object> AddCommentAsync(long postId, CommentCreateRequest request) =>
            _userService.AddCommentAsync(postId, request);

        public Task<object> GetCommentsAsync(long postId, int page = 1, int pageSize = 20) =>
            _userService.GetCommentsAsync(postId, page, pageSize);

        public Task<object> UpdateCommentAsync(long commentId, CommentCreateRequest request) =>
            _userService.UpdateCommentAsync(commentId, request);

        public Task DeleteMyUserCommentAsync(long commentId) =>
            _userService.DeleteCommentAsync(commentId);
        public Task<object> GetFeedAsync(int page = 1, int pageSize = 20) =>
            _userService.GetFeedAsync(page, pageSize);

        public Task<object> GetUserKpiAsync() =>
            _userService.GetKpiAsync();
        #endregion

        #region ---- Global search ----
        public Task<object> GlobalSearchAsync(string search,
                                                                     int page = 1,
                                                                     int pageSize = 20) =>
            _searchService.GlobalSearchAsync(search, page, pageSize);
        #endregion

        public Task<object> GetCommunityFollowersAsync(long communityId, long userId)
            => _adminService.GetCommunityFollowersAsync(communityId, userId);

        public Task<object> GetPostsTasksDependsOnRoleAsync(
            int page = 1,
            int pageSize = 20,
            string filter = null,
            string sort = null,
            string search = null, string status = null)
          => _adminService.GetPostsTasksDependsOnRoleAsync(page, pageSize, filter, sort, search, status);

        public Task<object> GetApprovedCommunitiesReadyToPublishAsync(
            int page = 1,
            int pageSize = 20,
            string filter = null,
            string sort = null,
            string search = null, string status = null)
            => _adminService.GetApprovedCommunitiesReadyToPublishAsync(page, pageSize, filter, sort, search, status);

        public Task<object> GetProfileMemberAsync(string userId,string companyId) =>
            _userService.GetProfileMemberAsync(userId, companyId);

        public Task DeleteHistoryById(long historyId) =>
            _userService.DeleteHistoryById(historyId);


        public Task DeleteAllHistory() =>
            _userService.DeleteAllHistory();


        public Task<object> GetSearchHistory() =>
            _userService.GetSearchHistory();

    }
}
