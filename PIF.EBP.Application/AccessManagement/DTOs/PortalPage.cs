using PIF.EBP.Application.Shared;

namespace PIF.EBP.Application.AccessManagement.DTOs
{
    public class PortalPage : ICacheItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string NameAr { get; set; }
        public string Link { get; set; }
    }
}
