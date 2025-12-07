using System.Collections.Generic;

namespace PIF.EBP.Core.FileManagement.DTOs
{
    public class FolderStructureRes
    {
        public int Id { get; set; }
        public string Path { get; set; }
        public string Name { get; set; }
        public string NameAr { get; set; }
        public string DisplayName { get; set; }
        public string DisplayNameAr { get; set; }
        public List<FolderStructure> SubFolders { get; set; } = new List<FolderStructure>();
    }

}
