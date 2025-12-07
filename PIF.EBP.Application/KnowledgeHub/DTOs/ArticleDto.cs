using PIF.EBP.Application.MetaData.DTOs;
using System;
using System.Collections.Generic;

namespace PIF.EBP.Application.KnowledgeHub.DTOs
{
    public class ArticleDto
    {
        public int ItemCount { get; set; }
        public List<Article> Articles { get; set; }
    }

    public class Article 
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string TitleAr { get; set; }
        public string Description { get; set; }
        public string DescriptionAr { get; set; }
        public DateTime CreatedDate { get; set; }
        public EntityReferenceDto CreatedBy { get; set; }
        public string Keywords { get; set; }
        public string[] MediaLinks { get; set; }
        public EntityReferenceDto Author { get; set; }
        public EntityOptionSetDto ArticleType { get; set; }
        public EntityReferenceDto ContentClassification { get; set; }
        public List<KnowledgeHubDocumentRsp> ReferenceFiles { get; set; }
        public List<KnowledgeHubDocumentRsp> MediaFiles { get; set; }
        public KnowledgeHubDocumentRsp CoverImage { get; set; }
        public bool IsPin { get; set; }
        public string ShortDescription { get; internal set; }
        public string ShortDescriptionAr { get; internal set; }
    }
}
