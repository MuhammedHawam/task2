using PIF.EBP.Application.MetaData.DTOs;
using System;
using System.Collections.Generic;

namespace PIF.EBP.Application.Hexa.DTOs
{
    public class RequestStepInStagesDto
    {
        public Guid? Id { get; set; } = null;
        public string Name { get; set; }
        public string NameAr { get; set; }
        public decimal StepNumber { get; set; }
        public string Instruction { get; set; }
        public string InstructionAr { get; set; }
        public string Summary { get; set; }
        public string SummaryAr { get; set; }
        public string RequestStepNumber { get; set; }
        public int? IsExternalUser { get; set; }
        public bool CanOpen { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public EntityReferenceDto Group { get; set; }
        public EntityOptionSetDto StateCode { get; set; }
        public EntityReferenceDto Status { get; set; }
        public EntityReferenceDto Customer { get; set; }
        public EntityReferenceDto AssignedTo { get; set; }
        public int? RoleTypeCode { get; set; }


    }
    public class HexaStageDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string NameAr { get; set; }
        public int StageNumber { get; set; }
        public string Description { get; set; }
        public string CompanyId { get; set; }
        public bool Disabled { get; set; }
        public int RequestStepOwnership { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public EntityReferenceDto ProcessTemplate { get; set; }
        public EntityOptionSetDto Status { get; set; }

    }
    public class StageDetailsDto
    {
        public HexaStageDto Stage { get; set; }
        public List<RequestStepInStagesDto> Steps { get; set; }

    }
}
