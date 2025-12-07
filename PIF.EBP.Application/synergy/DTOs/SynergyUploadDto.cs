using System;
using System.Collections.Generic;

namespace PIF.EBP.Application.Synergy.DTOs
{
    public class SynergyUploadRequestDto
    {
        public string SynergyRequestId { get; set; }
        public string CompanyId { get; set; }
        public string ContactId { get; set; }
        public Dictionary<string, object> AdditionalMetadata { get; set; }
    }

    public class SynergyUploadResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<SynergyFileInfo> UploadedFiles { get; set; }
    }

    public class SynergyFileInfo
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