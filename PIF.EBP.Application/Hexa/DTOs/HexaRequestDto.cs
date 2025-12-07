using Newtonsoft.Json.Linq;
using PIF.EBP.Application.ExternalFormConfiguration.DTOs;
using PIF.EBP.Application.MetaData.DTOs;
using System;
using System.Collections.Generic;

namespace PIF.EBP.Application.Hexa.DTOs
{
    public class HexaRequestDto
    {
        public Guid Id { get; set; }
        public string RequestId { get; set; }
        public string RequestName { get; set; }
        public string Title { get; set; }
        public string TitleAr { get; set; }
        public string Summary { get; set; }
        public string SummaryAr { get; set; }
        public DateTime? DueOn { get; set; }
        public DateTime CreationDate { get; set; }
        public EntityReferenceDto Stage { get; set; }
        public EntityReferenceDto Status { get; set; }
        public EntityReferenceDto Owner { get; set; }
        public byte[] OwnerImage { get; set; }
        public EntityReferenceDto ProcessTemplate { get; set; }
        public EntityReferenceDto CurrentStep { get; set; }
        public EntityReferenceDto Company { get; set; }
        public List<StageDetailsDto> Stages { get; set; }
        public List<dynamic> ExtensionObject { get; set; } = new List<dynamic>();
        public List<dynamic> RequestDocuments { get; set; } = new List<dynamic>();
        public JObject DynamicData { get; set; }
        public List<ExternalFormConfigDto> ExternalFormConfiguration { get; set; } = new List<ExternalFormConfigDto>();
    }
}
