namespace PIF.EBP.Core.FileManagement.DTOs
{
    public class DeleteFileRequest
    {
        public string SourceFilePath { get; set; }
    }

    public class DeleteFileResponse
    {
        public string DocumentPath { get; set; }
        public bool Deleted { get; set; }
        public string Status { get; set; }
        
    }
}
