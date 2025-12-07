using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIF.EBP.Application.PartnersHub.DTOs
{
    public class PartnersHubUploadRequestDto
    {
        public string InnovationRequestId { get; set; }
        public string CompanyId { get; set; }
        public string ContactId { get; set; }
        public Dictionary<string, object> AdditionalMetadata { get; set; }
    }

    public class PartnersHubUploadResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<PartnersHubFileInfo> UploadedFiles { get; set; }
    }

    public class PartnersHubFileInfo
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
