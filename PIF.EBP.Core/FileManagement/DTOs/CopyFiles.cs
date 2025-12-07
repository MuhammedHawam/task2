using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PIF.EBP.Core.FileManagement.DTOs
{
    public class CopyFiles
    {
        [Required]
        public Guid SourceObjectListId { get; set; }

        [Required]
        public Guid DestinationObjectId { get; set; }

        public bool MoveAll { get; set; }

        public List<string> ListFilesName { get; set; }
    }

    public class CopyFilesResponse
    {
        public Guid SourceObjectListId { get; set; }

        public int CopiedItemCount { get; set; }

        public int ItemCount { get; set; }
        public string FolderPath { get; set; }
    }
}
