using PIF.EBP.Application.MetaData.DTOs;
using PIF.EBP.Application.Networking.DTOs;
using System;
using System.Collections.Generic;
using PIF.EBP.Core.PartnersHub.DTOs;

namespace PIF.EBP.Application.Companies.DTOs
{
    /// <summary>
    /// Detailed company information for detail view
    /// </summary>
    public class CompanyDetailsDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string NameAr { get; set; }
        public byte[] Logo { get; set; }
        
        /// <summary>
        /// Industry information
        /// </summary>
        public string Industry { get; set; }
        public string IndustryAr { get; set; }
        
        /// <summary>
        /// Location information
        /// </summary>
        public string Location { get; set; }
        public string LocationAr { get; set; }
        
        /// <summary>
        /// Sector info (GICS Sector)
        /// </summary>
        public EntityReferenceDto Sector { get; set; }
        
        /// <summary>
        /// Representative/Point of Contact information
        /// </summary>
        public RepresentativeInfo Representative { get; set; }
        
        /// <summary>
        /// Full description (not truncated)
        /// </summary>
        public string Description { get; set; }
        public string DescriptionAr { get; set; }
        
        /// <summary>
        /// Activity statistics
        /// </summary>
        public int ChallengesCount { get; set; }
        public int CampaignsCount { get; set; }
        public int TotalActivity { get; set; }
        
        /// <summary>
        /// Full challenges and campaigns data from PartnersHub API
        /// </summary>
        public List<ChallengeCompanyDTO> Challenges { get; set; }
        public List<CampaignCompanyDTO> Campaigns { get; set; }
        
        /// <summary>
        /// Additional company details
        /// </summary>
        public string Website { get; set; }
        public DateTime? EstablishmentDate { get; set; }
        public DateTime? CreatedOn { get; set; }

        public CompanyDetailsDto()
        {
            Representative = new RepresentativeInfo();
            Challenges = new List<ChallengeCompanyDTO>();
            Campaigns = new List<CampaignCompanyDTO>();
        }
    }
}
