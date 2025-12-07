using System;
using System.Collections.Generic;
using static PIF.EBP.Application.Shared.Enums;

namespace PIF.EBP.Application.Companies.DTOs
{
    /// <summary>
    /// Request DTO for filtering, searching, and paginating companies
    /// </summary>
    public class CompanyRequestDto
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string SearchText { get; set; }
        
        /// <summary>
        /// Multi-select GICS sectors filter
        /// </summary>
        public List<Guid> SectorIds { get; set; }
        
        /// <summary>
        /// Multi-select cities filter
        /// </summary>
        public List<Guid> CityIds { get; set; }
        
        /// <summary>
        /// Multi-select regions filter
        /// </summary>
        public List<Guid> RegionIds { get; set; }
        
        public CompanySortOrder SortBy { get; set; }

        public CompanyRequestDto()
        {
            PageNumber = 1;
            PageSize = 10;
            SortBy = CompanySortOrder.MostActive;
            SectorIds = new List<Guid>();
            CityIds = new List<Guid>();
            RegionIds = new List<Guid>();
        }
    }
}
