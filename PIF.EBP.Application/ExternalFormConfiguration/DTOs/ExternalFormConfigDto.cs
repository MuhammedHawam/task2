using PIF.EBP.Application.MetaData.DTOs;
using System;

namespace PIF.EBP.Application.ExternalFormConfiguration.DTOs
{
    public class ExternalFormConfigDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Actions { get; set; }
        public string Metadata { get; set; }
        public string Grids { get; set; }
        public string SelectedEntities { get; set; }
        public string GridTargetEntity { get; set; }
        public EntityOptionSetDto Type { get; set; }
        public EntityOptionSetDto FormType { get; set; }
        public EntityReferenceDto ProcessTemplate { get; set; }
        public EntityReferenceDto ProcessStepTemplate { get; set; }
    }
    public class ExternalFormConfigSimplifiedDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public EntityOptionSetDto FormType { get; set; }
    }
}
