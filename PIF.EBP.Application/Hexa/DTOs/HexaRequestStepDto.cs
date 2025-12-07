using Microsoft.Xrm.Sdk;
using Newtonsoft.Json.Linq;
using PIF.EBP.Application.ExternalFormConfiguration.DTOs;
using PIF.EBP.Application.MetaData.DTOs;
using System;
using System.Collections.Generic;

namespace PIF.EBP.Application.Hexa.DTOs
{
    public class HexaRequestStepDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string RequestStepNumber { get; set; }
        public string LatestPortalComment { get; set; }
        public decimal StepNumber { get; set; }
        public bool CanUpdate { get; set; }
        public bool CanReassign { get; set; }
        public DateTime? DueOn { get; set; }
        public DateTime CreationDate { get; set; }
        public EntityReferenceDto Stage { get; set; }
        public EntityReferenceDto Status { get; set; }
        public EntityReferenceDto ExternalStepStatus { get; set; }
        public EntityOptionSetDto StateCode { get; set; }
        public JObject DynamicData { get; set; }
        public EntityReferenceDto Customer { get; set; }
        public EntityReferenceDto PortalContact  { get; set; }
        public EntityReference PortalRole  { get; set; }
        public EntityReference Department { get; set; }
        public EntityReference InitiatingStep { get; set; }
        public byte[] Entityimage { get; set; }
        public HexaProcessStepTemplate ProcessStep { get; set; }
        public MasterRequestDto MasterRequest { get; set; }
        public List<dynamic> ExtensionObject { get; set; } = new List<dynamic>();
        public List<HexaStepTransitionDto> StepTransition { get; set; } = new List<HexaStepTransitionDto> { };
        public List<dynamic> StepRequestDocuments { get; set; } = new List<dynamic>();
        public List<ExternalFormConfigDto> ExternalFormConfiguration { get; set; } = new List<ExternalFormConfigDto>();
        public int? RoleTypeCode { get; set; }
    }

    public class RequestStepAuthorizationNeedsDto
    {
        public EntityReferenceDto PortalContact { get; set; }
        public EntityOptionSetDto StateCode { get; set; }
        public EntityReference portalRoleOnProcessStepTemplate { get; set; }

    }
    public class MasterRequestDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string TitleAr { get; set; }
        public string Summary { get; set; }
        public string SummaryAr { get; set; }
        public string Purpose { get; set; }
        public string PurposeAr { get; set; }
    }
}
