using System;
using System.Collections.Generic;
namespace PIF.EBP.Core.PartnersHub.DTOs
{
    public class ChallengeByCompanyIdRequest
    {
        public List<Guid> CopmanyIds { get; set; }
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
    }

    public class ChallengeWithCampaignResponse
    {
        public PagingResult<ChallengeCompanyDTO> Challenges { get; set; }
        public PagingResult<CampaignCompanyDTO> Campaigns { get; set; }
    }

    public class PagingResult<T>
    {
        public List<T> Items { get; set; }
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public class ChallengeCompanyDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string DevCoName { get; set; }
        public string SectorName { get; set; }
        public string PriorityLevel { get; set; }  // Changed from int to string
        public string ChallengeStatus { get; set; }  // Changed from int to string
        public DateTime CreatedAt { get; set; }
        public Guid SourceCompanyId { get; set; }  // Added for company association
    }

    public class CampaignCompanyDTO
    {
        public Guid CampaignId { get; set; }
        public string CampaignName { get; set; }
        public string Description { get; set; }
        public DateTime? SubmissionDeadline { get; set; }
        public string CampaignStatus { get; set; }  // Changed from int to string
        public string CampaignType { get; set; }  // Changed from int to string
        public DateTime? LunchDate { get; set; }
        public List<Guid> SourceCompanyIds { get; set; }  // Added for company associations
    }

    /// <summary>
    /// Priority levels for challenges (string values from API)
    /// </summary>
    public enum PriorityLevel
    {
        Urgent = 1,
        High = 2,
        Medium = 3,
        Low = 4
    }

    /// <summary>
    /// Status of challenges (string values from API)
    /// </summary>
    public enum ChallengeStatus
    {
        Draft = 0,
        Archived = 1,
        Pending = 2,
        RevisionsRequest = 3,
        Approved = 4
    }

    /// <summary>
    /// Status of campaigns (string values from API)
    /// </summary>
    public enum CampaignStatus
    {
        Open = 0,
        Closed = 1,
        Upcoming = 2
    }

    /// <summary>
    /// Type of campaigns (string values from API)
    /// </summary>
    public enum CampaignType
    {
        Public = 0,
        Internal = 1
    }
}
