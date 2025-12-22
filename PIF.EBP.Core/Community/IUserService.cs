using PIF.EBP.Core.Community.DTOs;
using PIF.EBP.Core.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIF.EBP.Core.Community
{
    public interface IUserService : ITransientDependency
    {
        #region Community actions (user)

        Task<object> FollowCommunityAsync(long communityId);
        Task<object> SuggestCommunityAsync(CommunityCreateRequest request);

        #endregion

        #region Posts (CRUD + like / unlike)

        Task<object> CreatePostAsync(PostCreateRequest request);
        Task<object> GetPostAsync(long postId);
        Task<object> GetMyPostsAsync(int page = 1,
                                            int pageSize = 20,
                                            string filter = null,
                                            string sort = null,
                                            string search = null);
        Task<object> UpdatePostAsync(long postId, PostUpdateRequest request);
        Task DeletePostAsync(long postId);
        Task LikePostAsync(long postId);
        Task UnlikePostAsync(long postId);

        #endregion

        #region Comments (CRUD)

        Task<object> AddCommentAsync(long postId, CommentCreateRequest request);
        Task<object> GetCommentsAsync(long postId, int page = 1, int pageSize = 20);
        Task<object> UpdateCommentAsync(long commentId, CommentCreateRequest request);
        Task DeleteCommentAsync(long commentId);

        #endregion

        #region Feed & KPI

        Task<object> GetFeedAsync(int page = 1, int pageSize = 20);
        Task<object> GetKpiAsync();

        #endregion

        Task<object> GetProfileMemberAsync(string userId, string companyId);
        Task DeleteHistoryById(long historyId);
        Task DeleteAllHistory();
        Task<object> GetSearchHistory();


    }
}
