using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PIF.EBP.Core.FileManagement.DTOs
{
    public class DeleteDocumentsDto
    {
        [Required]
        public Guid RegardingObjectId { get; set; }

        [Required]
        public List<string> DocumentsName { get; set; }
    }

    public class DeleteDocumentsRes
    {
        public int DeletedCount { get; set; }
        public int ItemCount { get; set; }
        public string Description { get; set; }
    }
}
