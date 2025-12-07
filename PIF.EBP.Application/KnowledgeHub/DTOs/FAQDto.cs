using PIF.EBP.Application.MetaData.DTOs;
using System;

namespace PIF.EBP.Application.KnowledgeHub.DTOs
{
    public class FAQDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string TitleAr { get; set; }
        public string Description { get; set; }
        public string DescriptionAr { get; set; }
        public DateTime Created { get; set; }
        public string Query { get; set; }
        public string QueryAr { get; set; }
        public string Response { get; set; }
        public string ResponseAr { get; set; }
        public EntityOptionSetDto Category { get; set; }
        public EntityReferenceDto ContentClassification { get; set; }
    }
}
