using PIF.EBP.Application.Community;
using PIF.EBP.Core.Community.DTOs;
using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.WebAPI.Middleware.ActionFilter;
using System;
using System.Configuration;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;


[ApiResponseWrapper]
[RoutePrefix("community")] // Use "api/community" as the base route
public class CommunityController : ApiController
{
    private readonly ICommunityManagementService _cmService;

    public CommunityController()
    {
        _cmService = WindsorContainerProvider.Container.Resolve<ICommunityManagementService>()
                     ?? throw new InvalidOperationException("ICommunityManagementService not registered in Windsor.");

        bool.TryParse(ConfigurationManager.AppSettings["LogCommunityRequests"], out bool enableLogging);
    }


    #region ---- Admin Community Endpoints (api/community/admin/...) ------------------------------------------------

    /// <summary>
    /// GET /api/community/admin/{communityId} - Returns the full admin view of a community.
    /// </summary>
    [HttpGet]
    [Route("admin/communities/{communityId:long}")]
    public async Task<IHttpActionResult> GetAdminCommunity(long communityId)
    {
        var result = await _cmService.GetCommunityAsync(communityId);
        return Ok(result);
    }

    [HttpGet]
    [Route("admin/communities/{communityId:long}/followers/{userId:long}")]
    public async Task<IHttpActionResult> GetCommunityFollowers(long communityId, long userId)
    {
        var result = await _cmService.GetCommunityFollowersAsync(communityId, userId);
        return Ok(result);
    }

    [HttpGet]
    [Route("admin/posts/my-tasks")]
    public async Task<IHttpActionResult> GetPostsTasksDependsOnRole(int page = 1,
                                                              int pageSize = 20,
                                                              string filter = null,
                                                              string sort = null,
                                                              string search = null, string status = null)
    {
        var result = await _cmService.GetPostsTasksDependsOnRoleAsync(page, pageSize, filter, sort, search, status);
        return Ok(result);
    }

    [HttpGet]
    [Route("admin/communities/my-tasks")]
    public async Task<IHttpActionResult> GetApprovedCommunitiesReadyToPublish(int page = 1,
                                                          int pageSize = 20,
                                                          string filter = null,
                                                          string sort = null,
                                                          string search = null, string status = null)
    {
        var result = await _cmService.GetApprovedCommunitiesReadyToPublishAsync(page, pageSize, filter, sort, search, status);
        return Ok(result);
    }

    /// <summary>
    /// POST /api/community/admin/create - Creates a new community.
    /// </summary>
    [HttpPost]
    [Route("admin/communities")]
    public async Task<IHttpActionResult> CreateCommunity([FromBody] CommunityCreateRequest request)
    {
        var result = await _cmService.CreateCommunityAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// PUT /api/community/admin/{communityId} - Updates an existing community.
    /// </summary>
    [HttpPut]
    [Route("admin/communities/{communityId:long}")]
    public async Task<IHttpActionResult> UpdateCommunity(long communityId, [FromBody] CommunityUpdateRequest request)
    {
        var result = await _cmService.UpdateCommunityAsync(communityId, request);
        return Ok(result);
    }

    /// <summary>
    /// DELETE /api/community/admin/{communityId} - Deletes a community.
    /// </summary>
    [HttpDelete]
    [Route("admin/communities/{communityId:long}")]
    public async Task<IHttpActionResult> DeleteCommunity(long communityId)
    {
        await _cmService.DeleteCommunityAsync(communityId);
        return StatusCode(HttpStatusCode.NoContent);
    }

    /// <summary>
    /// POST /api/community/admin/{communityId}/approve - Approves a pending community.
    /// </summary>
    [HttpPut]
    [Route("admin/communities/{communityId:long}/approve")]
    public async Task<IHttpActionResult> ApproveCommunity(long communityId)
    {
        var result = await _cmService.ApproveCommunityAsync(communityId);
        return Ok(result);
    }

    /// <summary>
    /// POST /api/community/admin/{communityId}/archive - archives a pending community.
    /// </summary>
    [HttpPut]
    [Route("admin/communities/{communityId:long}/archive")]
    public async Task<IHttpActionResult> ArchiveCommunity(long communityId)
    {
        var result = await _cmService.ArchiveCommunityAsync(communityId);
        return Ok(result);
    }

    /// <summary>
    /// POST /api/community/admin/{communityId}/unarchive - unarchives a pending community.
    /// </summary>
    [HttpPut]
    [Route("admin/communities/{communityId:long}/unarchive")]
    public async Task<IHttpActionResult> UnArchiveCommunity(long communityId)
    {
        var result = await _cmService.UnArchiveCommunityAsync(communityId);
        return Ok(result);
    }

    /// <summary>
    /// POST /api/community/admin/{communityId}/reject - Rejects a pending community.
    /// </summary>
    [HttpPut]
    [Route("admin/communities/{communityId:long}/reject")]
    public async Task<IHttpActionResult> RejectCommunity(long communityId, [FromBody] CommunityRejectRequest request)
    {
        var result = await _cmService.RejectCommunityAsync(communityId, request);
        return Ok(result);
    }

    /// <summary>
    /// POST /api/community/admin/{communityId}/publish - Publishes a community.
    /// </summary>
    [HttpPut]
    [Route("admin/communities/{communityId:long}/publish")]
    public async Task<IHttpActionResult> PublishCommunity(long communityId, [FromBody] CommunityPublishRequest request)
    {
        var result = await _cmService.PublishCommunityAsync(communityId, request);
        return Ok(result);
    }

    /// <summary>
    /// GET /api/community/admin/all - Gets all communities.
    /// </summary>
    [HttpGet]
    [Route("admin/communities")]
    public async Task<IHttpActionResult> GetAllCommunities(int page = 1, int pageSize = 20)
    {
        var result = await _cmService.GetAllCommunitiesAsync(page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// GET /api/community/admin/pending - Gets pending communities with filtering.
    /// </summary>
    [HttpGet]
    [Route("admin/communities/pending")]
    public async Task<IHttpActionResult> GetPendingCommunities(int page = 1,
                                                               int pageSize = 20,
                                                               string filter = null,
                                                               string sort = null,
                                                               string search = null)
    {
        var result = await _cmService.GetPendingCommunitiesAsync(page, pageSize, filter, sort, search);
        return Ok(result);
    }

    /// <summary>
    /// GET /api/community/admin/kpi - Gets admin key performance indicators.
    /// </summary>
    [HttpGet]
    [Route("admin/kpi")]
    public async Task<IHttpActionResult> GetAdminKpi()
    {
        var result = await _cmService.GetAdminKpiAsync();
        return Ok(result);
    }

    /// <summary>
    /// GET /api/community/admin/posts/pending - Gets pending posts for moderation.
    /// </summary>
    [HttpGet]
    [Route("admin/posts/pending")]
    public async Task<IHttpActionResult> GetPendingPosts(int page = 1,
                                                         int pageSize = 20,
                                                         string filter = null,
                                                         string sort = null,
                                                         string search = null)
    {
        var result = await _cmService.GetPendingPostsAsync(page, pageSize, filter, sort, search);
        return Ok(result);
    }

    /// <summary>
    /// POST /api/posts/admin/{communityId}/archive - archives a post.
    /// </summary>
    [HttpPut]
    [Route("admin/posts/{communityId:long}/archive")]
    public async Task<IHttpActionResult> ArchivePosts(long communityId)
    {
        var result = await _cmService.ArchivePostsAsync(communityId);
        return Ok(result);
    }

    /// <summary>
    /// POST /api/posts/admin/{communityId}/unarchive - unarchives a post.
    /// </summary>
    [HttpPut]
    [Route("admin/posts/{communityId:long}/unarchive")]
    public async Task<IHttpActionResult> UnArchivePosts(long communityId)
    {
        var result = await _cmService.UnArchivePostsAsync(communityId);
        return Ok(result);
    }


    /// <summary>
    /// PUT /api/community/admin/posts/{postId}/status - Updates the status of a post.
    /// </summary>
    [HttpPut]
    [Route("admin/posts/{postId:long}/status")]
    public async Task<IHttpActionResult> UpdatePostStatus(long postId, [FromBody] PostStatusUpdateRequest request)
    {
        var result = await _cmService.UpdatePostStatusAsync(postId, request);
        return Ok(result);
    }

    /// <summary>
    /// DELETE /api/community/admin/posts/{postId} - Deletes a post (Admin override).
    /// </summary>
    [HttpDelete]
    [Route("admin/posts/{postId:long}")]
    public async Task<IHttpActionResult> DeleteAdminPost(long postId)
    {
        await _cmService.DeleteAdminPostAsync(postId);
        return StatusCode(HttpStatusCode.NoContent);
    }

    /// <summary>
    /// DELETE /api/community/admin/comments/{commentId} - Deletes a comment (Admin override).
    /// </summary>
    [HttpDelete]
    [Route("admin/comments/{commentId:long}")]
    public async Task<IHttpActionResult> AdminDeleteComment(long commentId)
    {
        await _cmService.DeleteCommentAsync(commentId);
        return StatusCode(HttpStatusCode.NoContent);
    }

    /// <summary>
    /// POST /api/community/admin/{communityId}/follow/{userId} - Allows admin to follow community on behalf of a user.
    /// </summary>
    [HttpPost]
    [Route("admin/{communityId:long}/follow/{userId:long}")]
    public async Task<IHttpActionResult> FollowCommunityAsAdmin(long communityId, long userId, [FromBody] CommunityFollowerCreateRequest request)
    {
        var result = await _cmService.FollowCommunityAsAdminAsync(communityId, userId, request);
        return Ok(result);
    }


    [HttpGet]
    [Route("admin/polls")]
    public async Task<IHttpActionResult> GetPollsList(int page = 1,
                                                             int pageSize = 20, string search = null,
                                                             string filter = null,
                                                             string sort = null)
    {
        var result = await _cmService.GetPollsListAsync(page, pageSize, search, filter, sort);
        return Ok(result);
    }

    [HttpPost]
    [Route("admin/polls")]
    public async Task<IHttpActionResult> CreatePoll([FromBody]CreatPollRequest request)
    {
        var result = await _cmService.CreatePollAsync(request);
        return Ok(result);
    }

    [HttpGet]
    [Route("admin/polls/{pollId:long}")]
    public async Task<IHttpActionResult> GetPollDetailsById(long pollId, string communityId)
    {
        var result = await _cmService.GetPollDetailsByIdAsync(pollId, communityId);
        return Ok(result);
    }

    [HttpPut]
    [Route("admin/polls/{pollId:long}")]
    public async Task<IHttpActionResult> UpdatePoll(long pollId, [FromBody] UpdatePollsRequest request)
    {
        var result = await _cmService.UpdatePollAsync(pollId, request);
        return Ok(result);
    }


    /// <summary>
    /// POST /api/posts/admin/{communityId}/archive - archives a post.
    /// </summary>
    [HttpPut]
    [Route("admin/polls/{pollId:long}/archive")]
    public async Task<IHttpActionResult> ArchivePoll(long pollId)
    {
        var result = await _cmService.ArchivePollAsync(pollId);
        return Ok(result);
    }

    /// <summary>
    /// POST /api/posts/admin/{communityId}/unarchive - unarchives a post.
    /// </summary>
    [HttpPut]
    [Route("admin/polls/{pollId:long}/unarchive")]
    public async Task<IHttpActionResult> UnArchivePoll(long pollId)
    {
        var result = await _cmService.UnArchivePollAsync(pollId);
        return Ok(result);
    }


    [HttpGet]
    [Route("admin/polls/{pollId:long}/statistics")]
    public async Task<IHttpActionResult> Getpollstatistics(long pollId)
    {
        var result = await _cmService.GetpollstatisticsAsync(pollId);
        return Ok(result);
    }

    #endregion


    #region ---- Public Community Endpoints (api/community/public/...) -----------------------------------------------

    /// <summary>
    /// GET /api/community/public/{communityId} - Gets read-only public details of a community.
    /// </summary>
    [HttpGet]
    [Route("communities/{communityId:long}")]
    public async Task<IHttpActionResult> GetPublicCommunity(long communityId)
    {
        var result = await _cmService.GetPublicCommunityAsync(communityId);
        return Ok(result);
    }

    /// <summary>
    /// GET /api/community/public/{communityId}/posts - Gets posts for a public community.
    /// </summary>
    [HttpGet]
    [Route("communities/{communityId:long}/posts")]
    public async Task<IHttpActionResult> GetPublicCommunityPosts(long communityId, int page = 1, int pageSize = 20)
    {
        var result = await _cmService.GetPublicCommunityPostsAsync(communityId, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// GET /api/community/public/{communityId}/members - Gets members of a public community.
    /// </summary>
    [HttpGet]
    [Route("communities/{communityId:long}/members")]
    public async Task<IHttpActionResult> GetPublicCommunityMembers(long communityId, int page = 1, int pageSize = 20)
    {
        var result = await _cmService.GetPublicCommunityMembersAsync(communityId, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// DELETE /api/community/public/{communityId}/unfollow - Unfollows a community (Public read-only action).
    /// </summary>
    [HttpPost]
    [Route("user/communities/{communityId:long}/unfollow")]
    public async Task<IHttpActionResult> UnfollowCommunity(long communityId)
    {
        await _cmService.UnfollowCommunityAsync(communityId);
        return Ok(HttpStatusCode.NoContent);
    }

    /// <summary>
    /// GET /api/community/public/suggested?search=... - Gets suggested communities.
    /// </summary>
    [HttpGet]
    [Route("search/suggestions")]
    public async Task<IHttpActionResult> GetSuggestedCommunities(string search)
    {
        var result = await _cmService.GetSuggestedCommunitiesAsync(search);
        return Ok(result);
    }

    /// <summary>
    /// GET /api/community/public/list - Gets a list of communities based on various filters.
    /// </summary>
    [HttpGet]
    [Route("communities")]
    public async Task<IHttpActionResult> GetCommunities(int page = 1,
                                                        int pageSize = 20,
                                                        bool? followedOnly = null,
                                                        bool? publishedOnly = null,
                                                        string filter = null,
                                                        string sort = null,
                                                        string search = null)
    {
        var result = await _cmService.GetCommunitiesAsync(page, pageSize, followedOnly, publishedOnly, filter, sort, search);
        return Ok(result);
    }

    #endregion


    #region ---- User Private Endpoints (api/community/user/...) -----------------------------------------------------

    /// <summary>
    /// POST /api/community/user/{communityId}/follow - Follows a community.
    /// </summary>
    [HttpPost]
    [Route("user/communities/{communityId:long}/follow")]
    public async Task<IHttpActionResult> FollowCommunity(long communityId)
    {
        var result = await _cmService.FollowCommunityAsync(communityId);
        return Ok(result);
    }

    /// <summary>
    /// POST /api/community/user/suggest - Suggests a new community.
    /// </summary>
    [HttpPost]
    [Route("user/communities/suggest")]
    public async Task<IHttpActionResult> SuggestCommunity([FromBody] CommunityCreateRequest request)
    {
        var result = await _cmService.SuggestCommunityAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// POST /api/community/user/posts/create - Creates a new post.
    /// </summary>
    [HttpPost]
    [Route("user/posts")]
    public async Task<IHttpActionResult> CreatePost([FromBody] PostCreateRequest request)
    {
        var result = await _cmService.CreatePostAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// GET /api/community/user/posts/{postId} - Gets details for one of the user's posts.
    /// </summary>
    [HttpGet]
    [Route("user/posts/{postId:long}")]
    public async Task<IHttpActionResult> GetMyPost(long postId)
    {
        var result = await _cmService.GetMyPostAsync(postId);
        return Ok(result);
    }

    /// <summary>
    /// GET /api/community/user/posts/mine - Gets a list of the user's posts.
    /// </summary>
    [HttpGet]
    [Route("user/posts")]
    public async Task<IHttpActionResult> GetMyPosts(int page = 1,
                                                    int pageSize = 20,
                                                    string filter = null,
                                                    string sort = null,
                                                    string search = null)
    {
        var result = await _cmService.GetMyPostsAsync(page, pageSize, filter, sort, search);
        return Ok(result);
    }

    /// <summary>
    /// PUT /api/community/user/posts/{postId} - Updates one of the user's posts.
    /// </summary>
    [HttpPut]
    [Route("user/posts/{postId:long}")]
    public async Task<IHttpActionResult> UpdateMyPost(long postId, [FromBody] PostUpdateRequest request)
    {
        var result = await _cmService.UpdateMyPostAsync(postId, request);
        return Ok(result);
    }

    /// <summary>
    /// DELETE /api/community/user/posts/{postId} - Deletes one of the user's posts.
    /// </summary>
    [HttpDelete]
    [Route("user/posts/{postId:long}")]
    public async Task<IHttpActionResult> DeleteMyPost(long postId)
    {
        await _cmService.DeleteMyPostAsync(postId);
        return StatusCode(HttpStatusCode.NoContent);
    }

    /// <summary>
    /// POST /api/community/user/posts/{postId}/like - Likes a post.
    /// </summary>
    [HttpPost]
    [Route("user/posts/{postId:long}/like")]
    public async Task<IHttpActionResult> LikePost(long postId)
    {
        await _cmService.LikePostAsync(postId);
        return StatusCode(HttpStatusCode.OK); // Return 200/NoContent
    }

    /// <summary>
    /// POST /api/community/user/posts/{postId}/unlike - Unlikes a post.
    /// </summary>
    [HttpPost]
    [Route("user/posts/{postId:long}/unlike")]
    public async Task<IHttpActionResult> UnlikePost(long postId)
    {
        await _cmService.UnlikePostAsync(postId);
        return StatusCode(HttpStatusCode.OK); // Return 200/NoContent
    }

    /// <summary>
    /// POST /api/community/user/posts/{postId}/comments - Adds a comment to a post.
    /// </summary>
    [HttpPost]
    [Route("user/posts/{postId:long}/comments")]
    public async Task<IHttpActionResult> AddComment(long postId, [FromBody] CommentCreateRequest request)
    {
        var result = await _cmService.AddCommentAsync(postId, request);
        return Ok(result);
    }

    /// <summary>
    /// GET /api/community/user/posts/{postId}/comments - Gets comments for a post.
    /// </summary>
    [HttpGet]
    [Route("user/posts/{postId:long}/comments")]
    public async Task<IHttpActionResult> GetComments(long postId, int page = 1, int pageSize = 20)
    {
        var result = await _cmService.GetCommentsAsync(postId, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// PUT /api/community/user/comments/{commentId} - Updates an existing comment.
    /// </summary>
    [HttpPut]
    [Route("user/comments/{commentId:long}")]
    public async Task<IHttpActionResult> UpdateComment(long commentId, [FromBody] CommentCreateRequest request)
    {
        var result = await _cmService.UpdateCommentAsync(commentId, request);
        return Ok(result);
    }

    [HttpDelete]
    [Route("user/comments/{commentId:long}")]
    public async Task<IHttpActionResult> UserDeleteComment(long commentId)
    {
        await _cmService.DeleteMyUserCommentAsync(commentId);
        return StatusCode(HttpStatusCode.NoContent);
    }

    /// <summary>
    /// GET /api/community/user/feed - Gets the personalized user feed.
    /// </summary>
    [HttpGet]
    [Route("user/feed")]
    public async Task<IHttpActionResult> GetFeed(int page = 1, int pageSize = 20)
    {
        var result = await _cmService.GetFeedAsync(page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// GET /api/community/user/kpi - Gets user key performance indicators.
    /// </summary>
    [HttpGet]
    [Route("user/kpi")]
    public async Task<IHttpActionResult> GetUserKpi()
    {
        var result = await _cmService.GetUserKpiAsync();
        return Ok(result);
    }

    [HttpGet]
    [Route("user/member-profile")]
    public async Task<IHttpActionResult> GetProfileMember(string userId,string companyId)
    {
        var result = await _cmService.GetProfileMemberAsync(userId, companyId);
        return Ok(result);
    }

    /// <summary>
    /// DELETE /api/community/user/posts/{postId} - Deletes one of the user's posts.
    /// </summary>
    [HttpDelete]
    [Route("user/search/history/clear")]
    public async Task<IHttpActionResult> DeleteAllHistory()
    {
        await _cmService.DeleteAllHistory();
        return StatusCode(HttpStatusCode.NoContent);
    }

    /// <summary>
    /// DELETE /api/community/user/posts/{postId} - Deletes one of the user's posts.
    /// </summary>
    [HttpDelete]
    [Route("user/search/history/{historyId:long}")]
    public async Task<IHttpActionResult> DeleteHistoryById(long historyId)
    {
        await _cmService.DeleteHistoryById(historyId);
        return StatusCode(HttpStatusCode.NoContent);
    }

    [HttpGet]
    [Route("user/search/history")]
    public async Task<IHttpActionResult> GetSearchHistory()
    {
        var result = await _cmService.GetSearchHistory();
        return Ok(result);
    }


    /// <summary>
    /// POST /api/community/user/polls/{pollId:long}/submit - Submit Answer to Poll.
    /// </summary>
    [HttpPost]
    [Route("user/polls/{pollId:long}/submit")]
    public async Task<IHttpActionResult> SubmitAnswerForPoll(long pollId, [FromBody] SubmitPollAnswerRequest request)
    {
        var result = await _cmService.SubmitAnswerForPollAsync(pollId, request);
        return Ok(result);
    }

    #endregion


    #region ---- Global Search Endpoints (api/community/search/...) --------------------------------------------------

    /// <summary>
    /// GET /api/community/search?search=... - Performs a global search across communities and posts.
    /// </summary>
    [HttpGet]
    [Route("search")]
    public async Task<IHttpActionResult> GlobalSearch(string search, int page = 1, int pageSize = 20)
    {
        var result = await _cmService.GlobalSearchAsync(search, page, pageSize);
        return Ok(result);
    }

    #endregion

    [HttpPost]
    [Route("notification-hub/send-email")]
    public async Task<IHttpActionResult> SendEmail(EmailNotificationModel model)
    {
        await _cmService.SendEmail(model);
        return Ok();
    }
}