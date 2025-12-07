using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIF.EBP.Core.Community.DTOs
{
    public enum CommunityStatus { PENDING, APPROVED, REJECTED, PUBLISHED }
    public enum PostStatus { PENDING, APPROVED, REJECTED, PUBLISHED }

    // -----------------------------------------------------------------
    // 1.1 Community DTOs
    // -----------------------------------------------------------------

    public class CommunityCreateRequest
    {
        public string name { get; set; }
        public string sector { get; set; }
        public string description { get; set; }
        public object logo { get; set; }
        public string reason { get; set; }
        public IEnumerable<long> companyIds { get; set; }
    }

    public class CommunityUpdateRequest : CommunityCreateRequest { }

    public class CommunityApproveRequest { } // no body – just a PUT

    public class CommunityRejectRequest
    {
        public string reason { get; set; }
    }

    public class CommunityPublishRequest
    {
        public string name { get; set; }
        public string sector { get; set; }
        public string description { get; set; }
        public object logo { get; set; }
        public string reason { get; set; }
        public IEnumerable<long> companyIds { get; set; }
    }


    public class CommunityFollowerCreateRequest
    {
        public bool restricted { get; set; }
    }

    // -----------------------------------------------------------------
    // 1.2 Post DTOs
    // -----------------------------------------------------------------

    public class PostCreateRequest
    {
        public string body { get; set; }
        public IEnumerable<long> communityIds { get; set; }
        public object media1 { get; set; }
        public object media2 { get; set; }
        public object media3 { get; set; }
        public object media4 { get; set; }
        public object file { get; set; }
        public IEnumerable<string> companyIds { get; set; }
        public string reason { get; set; }
    }

    public class PostUpdateRequest : PostCreateRequest { }

    public class PostStatusUpdateRequest
    {
        public string postStatus { get; set; }
        public string reason { get; set; }
    }

    // -----------------------------------------------------------------
    // 1.3 Comment DTOs
    // -----------------------------------------------------------------

    public class CommentCreateRequest
    {
        public string body { get; set; }
        public IEnumerable<string> companyIds { get; set; }
    }
}
