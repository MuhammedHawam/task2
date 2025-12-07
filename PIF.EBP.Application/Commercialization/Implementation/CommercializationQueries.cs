using PIF.EBP.Application.PortalConfiguration;
using PIF.EBP.Application.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PIF.EBP.Application.Commercialization.DTOs;
using PIF.EBP.Application.Commercialization.Interfaces;

namespace PIF.EBP.Application.Commercialization.Implementation
{
    public class CommercializationQueries : ICommercializationQueries
    {
        private readonly IPortalConfigAppService _portalConfigAppService;
        private readonly IESMService _eSMService;
        public CommercializationQueries(IPortalConfigAppService portalConfigAppService, IESMService eSMService)
        {
            _portalConfigAppService = portalConfigAppService;
            _eSMService=eSMService;
        }

        public async Task<List<T>> GetFromDataSource<T>(string entityName) where T : ICacheItem
        {
            var customizedItem = await GetServicesList();
            var cacheItem = new CommercializationCacheItem
            {
                Id = "",
                CustomizedItem = customizedItem
            };

            return new List<T> { (T)(ICacheItem)cacheItem };
        }

        public async Task<IEnumerable<TCacheItem>> GetItemFromDataSource<TCacheItem, TPrimaryKey>(string entityName) where TCacheItem : CacheItemBase<TPrimaryKey>
        {

            var customizedItem = await GetServicesList();
            var cacheItem = new CommercializationCacheItem
            {
                Id = "",
                CustomizedItem = customizedItem
            };
            return (IEnumerable<TCacheItem>)new List<CommercializationCacheItem>() { cacheItem };
        }

        private async Task<CustomizedItemDto> GetServicesList()
        {
            var configurations = _portalConfigAppService.RetrievePortalConfiguration(new List<string> { PortalConfigurations.CommercializationMainService });
            var mainServiceId = configurations.SingleOrDefault(a => a.Key == PortalConfigurations.CommercializationMainService)?.Value ?? string.Empty;


            var serviceResponseList = await _eSMService.GetServicesList(mainServiceId);
            CustomizedItemDto customizedItemDto = new CustomizedItemDto();
            var serviceItemDtoList = serviceResponseList.Select(service => new ServiceItemDto
            {
                Type = service.Type,
                ServiceId = service.SysId,
                Name = service.Name,
                ShortDescription = service.ShortDescription,
                Description = service.Description,
                Icon = service.Icon,
                Price = service.Price,
                RecurringPrice = service.RecurringPrice,
                ParentSysId = service.Parent?.SysId
            }).Distinct().ToList();

            customizedItemDto.Services = serviceItemDtoList.Where(x => x.Type == "Service").ToList();

            // Get all unique `SysId` to identify root categories
            var allSysIds = new HashSet<string>(serviceItemDtoList.Select(x => x.ServiceId));

            // Categories: Sub-Categories whose ParentSysId is null or does not exist in the list of `SysId`
            customizedItemDto.Categories = serviceItemDtoList
                .Where(x => x.Type == "Sub-Category" && (string.IsNullOrEmpty(x.ParentSysId) || !allSysIds.Contains(x.ParentSysId)))
                .Select(z => new CategoryItemDto
                {
                    Type = z.Type,
                    SysId = z.ServiceId,
                    Name = z.Name,
                    ShortDescription = z.ShortDescription,
                    Description = z.Description,
                    Icon = z.Icon,
                    ParentSysId = z.ParentSysId
                })
                .ToList();

            // SubCategories: Sub-Categories whose ParentSysId matches a Category's SysId
            customizedItemDto.SubCategories = serviceItemDtoList
                .Where(x => x.Type == "Sub-Category" && allSysIds.Contains(x.ParentSysId))
                .Select(z => new CategoryItemDto
                {
                    Type = z.Type,
                    SysId = z.ServiceId,
                    Name = z.Name,
                    ShortDescription = z.ShortDescription,
                    Description = z.Description,
                    Icon = z.Icon,
                    ParentSysId = z.ParentSysId
                })
                .ToList();
            await HandleServicesIcon(customizedItemDto);

            return customizedItemDto;
        }

        private async Task<string> GetAttachmentByItemId(string itemId)
        {
            var attachmentByte = await _eSMService.GetAttachmentByItemId(itemId);

            return attachmentByte;
        }
        private async Task HandleServicesIcon(CustomizedItemDto customizedItemDto)
        {
            foreach (var category in customizedItemDto.Categories)
            {
                if (!string.IsNullOrEmpty(category.Icon))
                {
                    category.Icon = await GetAttachmentByItemId(category.Icon);
                }
            }

            foreach (var subCategory in customizedItemDto.SubCategories)
            {
                if (!string.IsNullOrEmpty(subCategory.Icon))
                {
                    subCategory.Icon = await GetAttachmentByItemId(subCategory.Icon);
                }
            }
            foreach (var service in customizedItemDto.Services)
            {
                if (!string.IsNullOrEmpty(service.Icon))
                {
                    service.Icon = await GetAttachmentByItemId(service.Icon);
                }
            }
        }

    }
}
