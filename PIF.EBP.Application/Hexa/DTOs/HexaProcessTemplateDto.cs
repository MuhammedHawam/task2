using PIF.EBP.Application.ExternalFormConfiguration.DTOs;
using System;
using System.Collections.Generic;

namespace PIF.EBP.Application.Hexa.DTOs
{
    public class HexaProcessTemplateDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string NameEn { get; set; }
        public string NameAr { get; set; }
        public string Description { get; set; }
        public string DescriptionAr { get; set; }
        public double? EstimatedSla { get; set; }
        public DateTime CreationDate { get; set; }
        public List<ExternalFormConfigDto> ExternalForms { get; set; }
    }
}
