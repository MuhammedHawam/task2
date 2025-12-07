using PIF.EBP.Core.FileManagement.DTOs;
using System;
using System.Collections.Generic;

namespace PIF.EBP.Application.KnowledgeHub.DTOs
{
    public class ContentDto
    {
        public Guid CompanyId { get; set; }
        public Guid ContactId { get; set; }
        public string Title { get; set; }
        public string TitleAr { get; set; }
        public string Description { get; set; }
        public string DescriptionAr { get; set; }
        public List<UploadedDocDetails> Documents { get; set; }
    }
}
