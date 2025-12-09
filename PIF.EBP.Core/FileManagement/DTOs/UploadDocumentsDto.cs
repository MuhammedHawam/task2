using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PIF.EBP.Core.FileManagement.DTOs
{
    public class UploadDocumentsDto
    {
        public Guid RegardingObjectId { get; set; }
        public string TargetFolderURL { get; set; }
        public List<UploadedDocDetails> Documents { get; set; }
        public Dictionary<string, object> FileMetadata { get; set; }
        public bool IsPifNotify { get; set; }
        public bool IsPortalCall { get; set; }
        public int FileScanningResult { get; set; }
        public Guid KnowledgeItemId { get; set; }
        public Guid FeedbackId { get; set; }
        public string FeedbackFileExtension { get; set;}
        public string CompanyId { get; set; }
        public string ContactId { get; set; }
        public string EsmRequestId { get; set; }
        public string ReferenceId { get; set; }
        public string Description { get; set; }
        public string SynergyRequestId { get; set; }
        public string RequestId { get; set; } 
        public string ModuleName { get; set; }
    }

    public class UploadedDocDetails
    {
        [Required]
        public string DocumentName { get; set; }
        public string DocumentExtension { get; set; }
        public long DocumentSize { get; set; }
        [Required]
        public string DocumentContent { get; set; }
        public string FormDataKeyName { get; set; }
    }

    public class SPUploadReq
    {
        public string RelativePath { get; set; }
        public string FileName { get; set; }
        public string Base64File { get; set; }
    }

    public class DocumentHeader
    {
        public int ItemCount { get; set; }
        public bool SendForScanning { get; set; }
        public string FolderPath { get; set; }
        public List<DocumentDetails> Documents { get; set; } = new List<DocumentDetails>();
    }

    public class DocumentDetails
    {
        public string DocumentName { get; set; }
        public string DocumentPath { get; set; }
    }

    public class BulkUploadDocumentDto
    {
        public Guid CompanyId { get; set; }
        public string FolderName { get; set; }
        public bool IsPifNotify { get; set; }
        public int FileScanningResult { get; set; }
        public List<UploadedDocDetails> Documents { get; set; }
    }
    public class BulkUploadDocumentResponse
    {
        public bool FileUnderScanning { get; set; } = false;
        public List<DocumentDetails> UploadedDocuments { get; set; } = new List<DocumentDetails>();
    }

    public class UploadFilesResponse
    {
        public string DocumentName { get; set; }
        public string DocumentPath { get; set; }
        public bool Uploaded { get; set; }
        public string Status { get; set; }
        public bool SentForScanning { get; set; }
    }

    public class UploadFileRequestDto
    {
        public string ReferenceId { get; set; }
        public string CompanyId { get; set; }
        public string ContactId { get; set; }
        public string Description { get; set; }
    }
}
