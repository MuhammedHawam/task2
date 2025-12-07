using PIF.EBP.Application.MetaData.DTOs;
using System;
using System.Collections.Generic;

namespace PIF.EBP.Application.KnowledgeHub.DTOs
{
    public class TemplateDto
    {
        public int ItemCount { get; set; }
        public List<Template> Templates { get; set; }
    }

    public class Template
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string TitleAr { get; set; }
        public string Description { get; set; }
        public string DescriptionAr { get; set; }
        public DateTime CreatedDate { get; set; }
        public EntityReferenceDto CreatedBy { get; set; }
        public EntityReferenceDto ContentClassification { get; set; }
        public string TargetFolderURL { get; set; }
        public bool IsPin { get; set; }
        public KnowledgeHubDocumentDto DocumentDetails { get; set; }
        public string ShortDescription { get; set; }
        public string ShortDescriptionAr { get; set; }
    }
}
