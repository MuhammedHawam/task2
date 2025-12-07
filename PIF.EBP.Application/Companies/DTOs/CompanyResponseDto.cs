using System.Collections.Generic;

namespace PIF.EBP.Application.Companies.DTOs
{
    /// <summary>
    /// Response DTO containing paginated list of companies
    /// </summary>
    public class CompanyResponseDto
    {
        public List<CompanyDto> Companies { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }

        public CompanyResponseDto()
        {
            Companies = new List<CompanyDto>();
        }
    }
}
