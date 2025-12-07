using PIF.EBP.Application.MetaData.DTOs;
using PIF.EBP.Application.Shared.AppRequest;
using System;
using System.Collections.Generic;
using static PIF.EBP.Application.Shared.Enums;

namespace PIF.EBP.Application.PerformanceDashboard.DTOs
{
    public class CompanyKPIsMilestonesDto
    {
        public List<CompanyKPIsDto> CompanyKPIs { get; set; }
        public int KPIsCount { get; set; }
        public List<CompanyMilestonesDto> CompanyMilestones { get; set; }
        public int MilestonesCount { get; set; }
    }

    public class CompanyKPIsDto
    {
        public string Descripcion { get; set; }
        public string DescripcionAr { get; set; }
        public string MetricName { get; set; }
        public string MetricNameAr { get; set; }
        public string UnitOfMeasurement { get; set; }
        public string UnitOfMeasurementAr { get; set; }
        public EntityOptionSetDto MetricType { get; set; }
        public EntityOptionSetDto MetricStatus { get; set; }
        public string Weight { get; set; }
    }

    public class CompanyMilestonesDto
    {
        public string Name { get; set; }
        public string NameAr { get; set; }
        public DateTime? CompletionDate { get; set; }
        public EntityOptionSetDto Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? BaseLineEndDate { get; set; }
    }

    public class CompanyKPIsMilestonesRequestDto
    {
        public CompanyKPIsMilestonesRequestDto()
        {
            if (PagingRequest == null)
            {
                PagingRequest = new PagingRequest();
            }
        }

        public Guid CompanyId { get; set; }
        public int? Scope { get; set; } = (int)CompanyKPIsMilestonesScope.All;

        public PagingRequest PagingRequest { get; set; }
    }
}
