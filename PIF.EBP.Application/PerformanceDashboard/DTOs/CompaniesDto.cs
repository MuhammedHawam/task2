using System;
using System.Collections.Generic;

namespace PIF.EBP.Application.PerformanceDashboard.DTOs
{
    public class CompaniesDto
    {
        public List<Company> companies { get; set; }
        public int ItemCount { get; set; }
    }

    public class Company
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string NameAr { get; set; }
        public string HeadQuarter { get; set; }
        public string WebSite { get; set; }
        public byte[] EntityImage { get; set; }
        public string PointOfContactName { get; set; }
        public string PointOfContactNameAr { get; set; }
        public string PointOfContactEmail { get; set; }
        public string PointOfContactId { get; set; }
        public byte[] PointOfContactImage { get; set; }
        public ServiceProvider ServiceProvider { get; set; }
        public bool IsPin { get; set; }
    }

    public class ServiceProvider
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string NameAr { get; set; }
        public string Flag { get; set; }
    }
}
