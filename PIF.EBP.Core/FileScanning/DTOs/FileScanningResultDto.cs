using System;

namespace PIF.EBP.Core.FileScanning.DTOs
{
    public class Scan_results
    {
        public int scan_all_result_i { get; set; }
    }

    public class Metadata
    {
        public string RegardingObjectId { get; set; }
        public string CompanyId { get; set; }
        public string modified_by_contact_id { get; set; } 
        public string created_by_contact_Id { get; set; } 
        public DateTime modified_by_contact_DateTime { get; set; }
        public string IsPifNotify { get; set; }
        public string TargetFolderURL { get; set; }
        public string KnowledgeItemId { get; set; } 
        public bool IsPortalCall { get; set; }
        public string FeedbackId { get; set; }
        public string FeedbackFileExtension { get; set; }
        public string FolderName { get; set; }
        public int DocumentSize { get; set; }
        public string DocumentExtension { get; set; }
        public string EsmRequestId { get; set; }
        public string FormDataKeyName { get; set;}
    }

    public class File_info
    {
        public string display_name { get; set; }
    }

    public class FileScanningResultDto
    {
        public string data_id { get; set; }
        public Scan_results scan_results { get; set; }
        public Metadata metadata { get; set; }
        public File_info file_info { get; set; }
    }
}
