using System;

namespace PIF.EBP.Application.KnowledgeHub.DTOs
{
    public class KnowledgeHubDocumentRsp
    {
        public string DocumentName { get; set; }
        public string DocumentId { get; set; }
        public string DocumentPath { get; set; }
        public string DocumentType { get; set; }
        public DateTime DocumentCreatedOnInUTC { get; set; }
        public long DocumentSizeInBytes { get; set; }
    }
}
