using System;
using System.Collections.Generic;

namespace PIF.EBP.Application.Networking.DTOs
{
    public class NetworkingCompaniesRequestDto
    {
        public NetworkingCompaniesRequestDto()
        {
            PageNumber = 1;
            PageSize = 8; // Fixed at 8 per page per requirement
            SectorIds = new List<Guid>();
            CityIds = new List<Guid>();
            RegionIds = new List<Guid>();
            SortBy = NetworkingSortOrder.MostActive;
        }

        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string SearchText { get; set; }
        public List<Guid> SectorIds { get; set; } // Multi-select GICS sectors
        public List<Guid> CityIds { get; set; } // Multi-select cities (ntw_cities)
        public List<Guid> RegionIds { get; set; } // Multi-select regions
        public NetworkingSortOrder SortBy { get; set; }
    }

    public enum NetworkingSortOrder
    {
        MostActive = 0,  // Default: highest total of challenges + campaigns
        AlphabeticalAZ = 1,  // A–Z by company name
        Newest = 2,  // Most recently created
        Location = 3 // Sorted by location/city
    }
}
