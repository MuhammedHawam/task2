using PIF.EBP.Core.Community.DTOs;
using PIF.EBP.Core.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIF.EBP.Core.Community
{
    public interface ICommunityService : ITransientDependency
    {
        /// <summary>
        /// Gets the public details of a community.
        /// </summary>
        Task<object> GetCommunityAsync(long communityId);

        /// <summary>
        /// Returns the list of posts that belong to a community (paged).
        /// </summary>
        Task<object> GetCommunityPostsAsync(long communityId,
                                                   int page = 1,
                                                   int pageSize = 20);

        /// <summary>
        /// Returns the members of a community (paged).
        /// </summary>
        Task<object> GetCommunityMembersAsync(long communityId,
                                                     int page = 1,
                                                     int pageSize = 20);

        /// <summary>
        /// Un‑follow a community (public endpoint).
        /// </summary>
        Task UnfollowCommunityAsync(long communityId);

        /// <summary>
        /// Get up‑to‑4 suggested communities for the supplied search term.
        /// </summary>
        Task<object> GetSuggestedCommunitiesAsync(string search);

        /// <summary>
        /// Generic paged list of communities with optional filters.
        /// </summary>
        Task<object> GetCommunitiesAsync(int page = 1,
                                                     int pageSize = 20,
                                                     bool? followedOnly = null,
                                                     bool? publishedOnly = null,
                                                     string filter = null,
                                                     string sort = null,
                                                     string search = null);
    }
}
