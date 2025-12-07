using System;

namespace PIF.EBP.Core.FileManagement.DTOs
{
    public class RenameFilesDto
    {
        public string SourceFilePath { get; set; }
        public string NewFileName { get; set; }
        public int OverriddenAction { get; set; } = 0;
    }

    public class RenameFilesResponse
    {
        public string DocumentName { get; set; }
        public string DocumentPath { get; set; }
        public DateTime DocumentCreatedOnInUTC { get; set; }
        public long DocumentSizeInBytes { get; set; }
        public bool Renamed { get; set; }
        public string Status { get; set; }
    }
}
