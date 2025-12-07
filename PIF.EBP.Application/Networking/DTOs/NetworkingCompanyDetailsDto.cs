using PIF.EBP.Application.MetaData.DTOs;
using PIF.EBP.Core.PartnersHub.DTOs;
using System;
using System.Collections.Generic;

namespace PIF.EBP.Application.Networking.DTOs
{
    /// <summary>
    /// Detailed information for a single networking company
    /// Includes comprehensive data for the company details page
    /// </summary>
    public class NetworkingCompanyDetailsDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string NameAr { get; set; }
        public byte[] Logo { get; set; }

        // Company Basic Info
        public string Industry { get; set; }
        public string IndustryAr { get; set; }
        public string Location { get; set; }
        public string LocationAr { get; set; }

        // Sector Info (GICS Sector)
        public EntityReferenceDto Sector { get; set; }

        // Representative Information (from image)
        public RepresentativeInfo Representative { get; set; }

        // Company Description/Tagline (full text, not truncated)
        public string Description { get; set; }
        public string DescriptionAr { get; set; }

        // Activity Statistics
        public int ChallengesCount { get; set; }
        public int CampaignsCount { get; set; }
        public int TotalActivity { get; set; }

        // Full Challenges and Campaigns Data from PartnersHub API
        public List<ChallengeCompanyDTO> Challenges { get; set; }
        public List<CampaignCompanyDTO> Campaigns { get; set; }

        // Additional company details
        public string Website { get; set; }
        public DateTime? EstablishmentDate { get; set; }
        public DateTime? CreatedOn { get; set; }
    }

    /// <summary>
    /// Representative contact information from the company
    /// Based on the PNG screenshot showing "Sarah Johnson" details
    /// </summary>
    public class RepresentativeInfo
    {
        public string Name { get; set; }
        public string NameAr { get; set; }
        public string Position { get; set; }
        public string PositionAr { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Mobile { get; set; }
        public byte[] Photo { get; set; }
    }
}


