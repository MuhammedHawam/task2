using System;
using System.Collections.Generic;

namespace PIF.EBP.Application.InfraBase.DTOs
{
    public class InfraBaseUploadRequestDto
    {
        public string InfraBaseRequestId { get; set; }
        public string CompanyId { get; set; }
        public string ContactId { get; set; }
        public Dictionary<string, object> AdditionalMetadata { get; set; }
    }

    public class InfraBaseUploadResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<InfraBaseFileInfo> UploadedFiles { get; set; }
    }

    public class InfraBaseFileInfo
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string SharePointUrl { get; set; }
        public long FileSize { get; set; }
        public bool Uploaded { get; set; }
        public string Status { get; set; }
        public DateTime UploadedOn { get; set; }
    }
}
