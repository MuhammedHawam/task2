using PIF.EBP.Application.MetaData.DTOs;
using System;

namespace PIF.EBP.Application.Hexa.DTOs
{
    public class HexaProcessStepTemplate
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string NameAR { get; set; }
        public decimal StepNumber { get; set; }
        public string Instruction { get; set; }
        public string InstructionAr { get; set; }
        public string Summary { get; set; }
        public string SummaryAr { get; set; }
        public EntityReferenceDto Stage { get; set; }
        public EntityReferenceDto Group { get; set; }
        public EntityReferenceDto ProcessTemplate { get; set; }
        public bool IsExternal { get; set; }

    }
}
