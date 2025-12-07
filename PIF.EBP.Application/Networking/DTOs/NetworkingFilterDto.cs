using System;

namespace PIF.EBP.Application.Networking.DTOs
{
    /// <summary>
    /// DTO representing a city for networking filters
    /// </summary>
    public class NetworkingCityDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string NameAr { get; set; }
    }

    /// <summary>
    /// DTO representing a region for networking filters
    /// </summary>
    public class NetworkingRegionDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string NameAr { get; set; }
    }

    /// <summary>
    /// DTO representing a GICS sector for networking filters
    /// </summary>
    public class NetworkingSectorDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string NameAr { get; set; }
    }

    /// <summary>
    /// Response containing all filter options for networking companies
    /// </summary>
    public class NetworkingFiltersResponseDto
    {
        public System.Collections.Generic.List<NetworkingCityDto> Cities { get; set; }
        public System.Collections.Generic.List<NetworkingRegionDto> Regions { get; set; }
        public System.Collections.Generic.List<NetworkingSectorDto> Sectors { get; set; }

        public NetworkingFiltersResponseDto()
        {
            Cities = new System.Collections.Generic.List<NetworkingCityDto>();
            Regions = new System.Collections.Generic.List<NetworkingRegionDto>();
            Sectors = new System.Collections.Generic.List<NetworkingSectorDto>();
        }
    }
}
