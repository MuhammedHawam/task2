using PIF.EBP.Application.Shared;
using System.Collections.Generic;

namespace PIF.EBP.Application.Commercialization.DTOs
{
    public class CustomizedItemDto : ICacheItem
    {
        public CustomizedItemDto()
        {
            Categories = new List<CategoryItemDto>();
            SubCategories = new List<CategoryItemDto>();
            Services = new List<ServiceItemDto>();
        }
        public List<CategoryItemDto> Categories { get; set; }
        public List<CategoryItemDto> SubCategories { get; set; }
        public List<ServiceItemDto> Services { get; set; }
    }
    public class ServiceItemDto
    {
        public string Type { get; set; }
        public string ServiceId { get; set; }
        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string Price { get; set; }
        public string RecurringPrice { get; set; }
        public string ParentSysId { get; set; }
    }
    public class CategoryItemDto
    {
        public string Type { get; set; }
        public string SysId { get; set; }
        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string ParentSysId { get; set; }
    }
}
