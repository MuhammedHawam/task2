using System.Collections.Generic;

namespace PIF.EBP.Core.FileManagement.DTOs
{
    public class FolderStructure
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string NameAr { get; set; }
        public string DisplayName { get; set; }
        public string DisplayNameAr { get; set; }
        public string Order { get; set; }
        public string PermissionName { get; set; }
        public FolderOperation Operations { get; set; } = new FolderOperation();
    }
    public class FolderOperation
    {
        public bool CanUpload { get; set; }
        public bool CanRename { get; set; }
        public bool CanDownload { get; set; }
        public bool CanCopy { get; set; }
        public bool CanMove { get; set; }
        public bool CanDelete { get; set; }
    }

    public class FolderAndSubFolderConfig
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string NameAr { get; set; }
        public string DisplayName { get; set; }
        public string DisplayNameAr { get; set; }
        public List<SubFolderConfig> SubFolders { get; set; }
    }
    public class SubFolderConfig
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string NameAr { get; set; }
        public string Order { get; set; }
        public string DisplayName { get; set; }
        public string DisplayNameAr { get; set; }
        public string PermissionName { get; set; }
    }

}
