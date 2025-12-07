using PIF.EBP.Application.MetaData.DTOs;
using System;
using System.Collections.Generic;

namespace PIF.EBP.Application.Networking.DTOs
{
    public class NetworkingCompaniesResponseDto
    {
        public List<NetworkingCompanyDto> Companies { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class NetworkingCompanyDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string NameAr { get; set; }
        public byte[] Logo { get; set; }

        // Sector info (GICS Sector - ntw_gicssector)
        public EntityReferenceDto Sector { get; set; }

        // Location info split into City and Region name
        public string City { get; set; }
        public string RegionName { get; set; }

        // Activity counters
        public int ChallengesCount { get; set; } // Count from pwc_opportunity where pwc_regardingcompanyid = accountid
        public int CampaignsCount { get; set; } // Placeholder - needs clarification on entity name
        public int TotalActivity { get; set; } // Sum of challenges + campaigns

        // Description/Tagline (truncated to150 chars)
        public string Tagline { get; set; }
        public string TaglineAr { get; set; }

        // For sorting
        public DateTime? CreatedOn { get; set; }
    }
}
