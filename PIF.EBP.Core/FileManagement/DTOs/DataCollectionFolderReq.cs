namespace PIF.EBP.Core.FileManagement.DTOs
{
    public class DataCollectionFolderReq
    {
        public string CompanyLibDisplayName{ get; set; }
        public string CompanyId { get; set; }
        public string ContactId { get; set; }
        public string CompanyFolderStructure { get; set; }
        public string BoardMemberFolderStructure { get; set; }
        public bool IsBoardMember { get; set; }
    }
}
