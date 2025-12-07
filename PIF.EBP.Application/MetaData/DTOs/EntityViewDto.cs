using PIF.EBP.Application.Shared;

namespace PIF.EBP.Application.MetaData.DTOs
{
    public class EntityViewDto: ICacheItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool IsDefault { get; set; }
        public bool IsManaged { get; set; }
        public int? StatusCode { get; set; }
        public int QueryType { get; set; }
    }
}
