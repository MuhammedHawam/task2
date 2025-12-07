using System;

namespace PIF.EBP.Application.KnowledgeHub.DTOs
{
    public class KnowledgeHubDocumentDto
    {
        public string DocumentName { get; set; }
        public string DocumentPath { get; set; }
        public string DocumentType { get; set; }
        public DateTime DocumentCreatedOnInUTC { get; set; }
        public string DocumentCreatedBy { get; set; }
        public string DocumentCreatedByAr { get; set; }
        public long DocumentSizeInBytes { get; set; }

    }
}
