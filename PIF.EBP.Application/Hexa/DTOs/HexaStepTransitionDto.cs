using PIF.EBP.Application.MetaData.DTOs;
using System;

namespace PIF.EBP.Application.Hexa.DTOs
{
    public class HexaStepTransitionDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ActionLabel { get; set; }
        public bool IsCommentMandatory { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreationDate { get; set; }
        public EntityReferenceDto Transition { get; set; }
        public EntityReferenceDto ProcessStep { get; set; }
        public EntityReferenceDto ProcessTemplate { get; set; }
        public EntityReferenceDto RequestStatusExternal { get; set; }
        public EntityReferenceDto RequestStatusInternal { get; set; }
    }
}
