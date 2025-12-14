using PIF.EBP.Core.Community.DTOs;
using PIF.EBP.Core.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIF.EBP.Core.Community
{
    public interface IAdminService : ITransientDependency
    {
        #region Communities (admin)

        /// <summary>
        /// Retrieves a single community by its identifier.
        /// </summary>
        Task<object> GetCommunityAsync(long communityId);

        /// <summary>
        /// Creates a new community.
        /// </summary>
        Task<object> CreateCommunityAsync(CommunityCreateRequest request);

        /// <summary>
        /// Updates an existing community.
        /// </summary>
        Task<object> UpdateCommunityAsync(long communityId, CommunityUpdateRequest request);

        /// <summary>
        /// Deletes a community.
        /// </summary>
        Task DeleteCommunityAsync(long communityId);

        /// <summary>
        /// Approves a pending community.
        /// </summary>
        Task<object> ApproveCommunityAsync(long communityId);
        /// <summary>
        /// Archive a community.
        /// </summary>
        Task<object> ArchiveCommunityAsync(long communityId);
        /// <summary>
        /// UnArchive a community.
        /// </summary>
        Task<object> UnArchiveCommunityAsync(long communityId);


        /// <summary>
        /// Rejects a pending community.
        /// </summary>
        Task<object> RejectCommunityAsync(long communityId, CommunityRejectRequest request);

        /// <summary>
        /// Publishes an approved community.
        /// </summary>
        Task<object> PublishCommunityAsync(long communityId, CommunityPublishRequest request);

        /// <summary>
        /// Returns a paginated list of **all** communities (admin view).
        /// </summary>
        Task<object> GetAllCommunitiesAsync(int page = 1, int pageSize = 20);

        /// <summary>
        /// Returns a paginated list of **pending** communities with optional filtering/sorting/search.
        /// </summary>
        Task<object> GetPendingCommunitiesAsync(
            int page = 1,
            int pageSize = 20,
            string filter = null,
            string sort = null,
            string search = null);

        /// <summary>
        /// Returns the list of communities that are **approved and ready to be published** (my‑tasks view).
        /// </summary>
        Task<object> GetApprovedCommunitiesReadyToPublishAsync(
            int page = 1,
            int pageSize = 20,
            string filter = null,
            string sort = null,
            string search = null);

        /// <summary>
        /// Retrieves the followers for a specific community (admin can query a single user or all followers).
        /// </summary>
        Task<object> GetCommunityFollowersAsync(long communityId, long userId);

        /// <summary>
        /// Returns admin‑level KPI data.
        /// </summary>
        Task<object> GetAdminKpiAsync();

        #endregion

        #region Posts (admin)

        /// <summary>
        /// Returns a paginated list of **pending** posts with optional filtering/sorting/search.
        /// </summary>
        Task<object> GetPendingPostsAsync(
            int page = 1,
            int pageSize = 20,
            string filter = null,
            string sort = null,
            string search = null);


        /// <summary>
        /// Archive a post.
        /// </summary>
        Task<object> ArchivePostsAsync(long postId);


        /// <summary>
        /// UnArchive a post.
        /// </summary>
        Task<object> UnArchivePostsAsync(long postId);

        /// <summary>
        /// Updates the status of a post (e.g., approve, reject, publish, etc.).
        /// </summary>
        Task<object> UpdatePostStatusAsync(long postId, PostStatusUpdateRequest request);

        /// <summary>
        /// Deletes a post.
        /// </summary>
        Task DeletePostAsync(long postId);

        /// <summary>
        /// Returns the list of post‑tasks that depend on the current user's role (my‑tasks view).
        /// </summary>
        Task<object> GetPostsTasksDependsOnRoleAsync(
            int page = 1,
            int pageSize = 20,
            string filter = null,
            string sort = null,
            string search = null);

        #endregion

        #region Comments (admin)

        /// <summary>
        /// Deletes a comment.
        /// </summary>
        Task DeleteCommentAsync(long commentId);

        #endregion

        #region Followers (admin)

        /// <summary>
        /// Adds a follower to a community on behalf of an admin.
        /// </summary>
        Task<object> FollowCommunityAsync(
            long communityId,
            long userId,
            CommunityFollowerCreateRequest request);

        #endregion
    }
}
