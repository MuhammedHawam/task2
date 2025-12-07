using PIF.EBP.Application.MetaData.DTOs;
using System;

namespace PIF.EBP.Application.Companies.DTOs
{
    /// <summary>
    /// Basic company information for list view
    /// </summary>
    public class CompanyDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string NameAr { get; set; }
        public byte[] Logo { get; set; }
        
        /// <summary>
        /// Sector info (GICS Sector)
        /// </summary>
        public EntityReferenceDto Sector { get; set; }
        
        /// <summary>
        /// City name
        /// </summary>
        public string City { get; set; }
        
        /// <summary>
        /// Region name
        /// </summary>
        public string RegionName { get; set; }
        
        /// <summary>
        /// Tagline/Description (truncated to 150 chars)
        /// </summary>
        public string Tagline { get; set; }
        public string TaglineAr { get; set; }
        
        /// <summary>
        /// Created date for sorting
        /// </summary>
        public DateTime? CreatedOn { get; set; }
        
        /// <summary>
        /// Activity counters (optional - from PartnersHub API)
        /// </summary>
        public int ChallengesCount { get; set; }
        public int CampaignsCount { get; set; }
        public int TotalActivity { get; set; }
    }
}
