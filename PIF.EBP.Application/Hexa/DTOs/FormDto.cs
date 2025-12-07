using System;
using System.Collections.Generic;

namespace PIF.EBP.Application.Hexa.DTOs
{
    public class FormDto
    {
        public Guid? ProcessTemplateId { get; set; }
        public Guid? TransitionId { get; set; }
        public Guid? RequestStepId { get; set; }
        public bool? IsReassign { get; set; } = false;
        public bool IsSubmit { get; set; } = false;
        public List<FormEntities> FormEntities { get; set; }
    }
    public class FormEntities
    {
        public string EntityName { get; set; }
        public string RelationshipName { get; set; }
        public Guid EntityId { get; set; } = Guid.Empty;
        public List<EntityFields> Fields { get; set; }
    }
    public class EntityFields
    {
        public string AttributeName { get; set; }
        public string AttributeValue { get; set; }
        public string RefEntityName { get; set; }
    }
}
