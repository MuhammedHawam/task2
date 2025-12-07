using PIF.EBP.Application.MetaData.DTOs;

namespace PIF.EBP.Application.Requests.DTOs
{
    public class HexaRequestStepDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string RequestStepNumber { get; set; }
        public decimal StepNumber { get; set; }
        public string CreationDate { get; set; }
        public EntityReferenceDto Stage { get; set; }
        public EntityReferenceDto Status { get; set; }
        public EntityReferenceDto Owner { get; set; }
        public EntityReferenceDto ProcessStep { get; set; }
    }
}
