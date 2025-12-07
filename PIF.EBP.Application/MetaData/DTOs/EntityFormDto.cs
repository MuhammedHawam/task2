using PIF.EBP.Application.Shared;

namespace PIF.EBP.Application.MetaData.DTOs
{
    public class EntityFormDto: ICacheItem
    {
        public string Id { get; set; }
        public string FormXml { get; set; }
        public string Formjson { get; set; }
    }
}
