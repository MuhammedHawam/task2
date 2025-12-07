using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PIF.EBP.Core.FileManagement.DTOs
{
    public enum SortOrder
    {
        Ascending = 0,
        Descending = 1
    }

    public class GetDocumentsDto
    {
        public Guid RegardingObjectId { get; set; }
        public bool? ByRequest { get; set; } = false;

    }

    public class GetDocumentDto
    {
        [Required]
        public Guid RegardingObjectId { get; set; }
        [Required]
        public string DocumentName { get; set; }
    }

    public class GetDocumentListRspBase
    {
        public int ItemCount { get; set; }
        public List<GetDocumentListRsp> GetDocumentListRsp { get; set; }
        public string Description { get; set; }
    }

    public class GetDocumentRsp
    {
        public int ItemCount { get; set; }

        public byte[] DocumentContent { get; set; }
    }

    public class GetDocumentListRsp
    {
        public string DocumentName { get; set; }
        public string DocumentId { get; set; }
        public string DocumentPath { get; set; }
        public string DocumentType { get; set; }
        public Guid? EvaluatedAtStep { get; set; }
        public string EvaluatedStepName { get; set; }
        public Guid? RequestDocumentId { get; set; }
        public DateTime DocumentModifiedOnInUTC { get; set; }
        public string DocumentModifiedBy { get; set; }
        public bool IsExternalModifiedByUser { get; set; }
        public string DocumentModifiedByAr { get; set; }
        public DateTime DocumentCreatedOnInUTC { get; set; }
        public string DocumentCreatedBy { get; set; }
        public string DocumentCreatedByAr { get; set; }
        public long DocumentSizeInBytes { get; set; }
        public Dictionary<string, object> DocumentMetadata { get; set; }
        public string FolderName { get; set; }
        public string FolderId { get; set; }
        public string SubFolderName { get; set; }
        public string SubFolderId { get; set; }
        public FolderOperation Operations { get; set; } = new FolderOperation();

    }

    public class SearchDocumentListDto
    {
        public SearchDocumentListDto()
        {
            if (PagingRequest == null)
            {
                PagingRequest = new PagingRequest();
            }
        }

        public string targetFolderURL { get; set; }
        public string searchText { get; set; }
        public PagingRequest PagingRequest { get; set; }
    }

    public class GetDocumentListDto
    {
        public GetDocumentListDto()
        {
            if (PagingRequest == null)
            {
                PagingRequest = new PagingRequest();
            }
        }

        public string targetFolderURL { get; set; }
        public string ContactId { get; set; }
        public string CompanyId { get; set; }
        public PagingRequest PagingRequest { get; set; }
    }

    public class PagingRequest
    {
        public int PageNo { get; set; }
        private int pageSize;
        public int PageSize
        {
            get => pageSize <= 0 ? 10 : pageSize;
            set => pageSize = value;
        }
        public string SortField { get; set; }
        public SortOrder SortOrder { get; set; }
    }

    public class FileMetadata
    {
        public bool ExternalModifiedBy { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedByAr { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedByAr { get; set; }
    }
    public class RequestReferenceDocument
    {
        public Guid? RequestDocumentId { get; set; }
        public Guid? EvaluatedAtStep { get; set; }
        public string EvaluatedStepName { get; set; }
        public string ListName { get; set; }

    }
}
