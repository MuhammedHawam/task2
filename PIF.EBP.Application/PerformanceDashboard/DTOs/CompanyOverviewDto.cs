using PIF.EBP.Application.MetaData.DTOs;
using System;
using System.Collections.Generic;

namespace PIF.EBP.Application.PerformanceDashboard.DTOs
{
    public class CompanyOverviewDto
    {
        public string CompanyName { get; set; }
        public string CompanyNameAr { get; set; }
        public byte[] CompanyImage { get; set; }
        public string Website { get; set; }
        public string Address { get; set; }
        public string Overview { get; set; }
        public string OverviewAr { get; set; }
        public ServiceProvider ServiceProvider { get; set; }
        public DateTime? EstablishmentDate { get; set; }
        public string PointOfContactName { get; set; }
        public string PointOfContactNameAr { get; set; }
        public byte[] PointOfContactImage { get; set; }
        public string PointOfContactEmail { get; set; }
        public string PointOfContactId { get; set; }
        public List<ExecutiveManagementDto> ExecutiveManagement { get; set; }
        public RelationshipManagerDto RelationshipManager { get; set; }
}

    public class ExecutiveManagementDto
    {
        public string Name { get; set; }
        public string NameAr { get; set; }
        public byte[] entityImage { get; set; }
        public EntityReferenceDto Position { get; set; }
    }
    public class RelationshipManagerDto
    {
        public string SystemUserId { get; set; }
        public string FullName { get; set; }
        public string FullNameAr { get; set; }
    }
}
