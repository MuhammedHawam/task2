using System;

namespace PIF.EBP.Core.FileManagement.DTOs
{
    public class MoveFilesDto
    {
        public string SourceFilePath { get; set; }
        public string DestinationFilePath { get; set; }
        public int OverriddenAction { get; set; } = 0;
    }

    public class MoveFilesResponse
    {
        public string DocumentName { get; set; }
        public string DocumentPath { get; set; }
        public DateTime DocumentCreatedOnInUTC { get; set; }
        public long DocumentSizeInBytes { get; set; }
        public bool Moved { get; set; }
        public string Status { get; set; }
    }
}
