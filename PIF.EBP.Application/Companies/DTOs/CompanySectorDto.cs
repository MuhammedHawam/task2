using System;

namespace PIF.EBP.Application.Companies.DTOs
{
    /// <summary>
    /// DTO representing a GICS sector for company filters
    /// </summary>
    public class CompanySectorDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string NameAr { get; set; }
    }
}
