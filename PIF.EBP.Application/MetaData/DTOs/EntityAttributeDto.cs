using PIF.EBP.Application.Shared;
using System.Collections.Generic;

namespace PIF.EBP.Application.MetaData.DTOs
{
    public class EntityAttributeDto: ICacheItem
    {
        public string MetadataId { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string DisplayName { get; set; }
        public int? ColumnNumber { get; set; }
        public bool? IsRequiredForForm { get; set; }
        public string LookupTarget { get; set; }
        public string DateTimeFormat { get; set; }

        public Dictionary<string, int> Validations { get; set; }
        public Dictionary<string, int> Options { get; set; }

    }
}
