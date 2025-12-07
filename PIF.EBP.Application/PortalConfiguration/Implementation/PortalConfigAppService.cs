using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using PIF.EBP.Application.Cache;
using PIF.EBP.Application.EntitiesCache;
using PIF.EBP.Application.EntitiesCache.DTOs;
using PIF.EBP.Application.PortalConfiguration.DTOs;
using PIF.EBP.Application.Shared;
using PIF.EBP.Application.Shared.Helpers;
using PIF.EBP.Core.CRM;
using PIF.EBP.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PIF.EBP.Application.PortalConfiguration.Implementation
{
    public class PortalConfigAppService : IPortalConfigAppService
    {
        private readonly IEntitiesCacheAppService _entitiesCacheAppService;
        private readonly ICrmService _crmService;
        private readonly ICacheAppService _cacheAppService;

        public PortalConfigAppService(IEntitiesCacheAppService entitiesCacheAppService,
                                    ICrmService crmService,
                                    ICacheAppService cacheAppService)
        {
            _entitiesCacheAppService= entitiesCacheAppService;
            _crmService = crmService;
            _cacheAppService = cacheAppService;
        }

        public List<PortalConfigDto> RetrievePortalConfiguration(List<string> keys)
        {
            var portalConfigs = Task.Run(async () => await _entitiesCacheAppService.RetrievePortalconfigurations()).GetAwaiter().GetResult();
            portalConfigs=portalConfigs.Where(x => keys.Contains(x.Key)).ToList();
            return portalConfigs;
        }
        public PortalConfigDto RetrievePortalConfigurationByValue(string value)
        {
            var portalConfigs = Task.Run(async () => await _entitiesCacheAppService.RetrievePortalconfigurations()).GetAwaiter().GetResult();
            var portalConfig = portalConfigs?.FirstOrDefault(x => x.Value==value)??null;
            return portalConfig;
        }
        public async Task<CreateOrUpdatePortalConfigDto> CreateOrUpdatePortalConfiguration(CreateOrUpdatePortalConfigDto createOrUpdatePortalConfigDto)
        {
            try
            {
                var query = new QueryExpression(EntityNames.Portalconfiguration)
                {
                    ColumnSet = new ColumnSet("hexa_portalconfigurationid", "hexa_name", "hexa_value", "pwc_valuear", "hexa_typetypecode", "statecode"),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                    {
                        new ConditionExpression("hexa_name", ConditionOperator.Equal, createOrUpdatePortalConfigDto.Key),
                        new ConditionExpression("statecode", ConditionOperator.Equal, 0)
                    }
                    }
                };
                var portalConfig = _crmService.GetInstance().RetrieveMultiple(query).Entities.FirstOrDefault();

                var entity = new Entity(EntityNames.Portalconfiguration);
                entity["hexa_name"] = createOrUpdatePortalConfigDto.Key;
                entity["hexa_value"] = createOrUpdatePortalConfigDto.Value;
                entity["pwc_valuear"] = createOrUpdatePortalConfigDto.ValueAr;

                entity["hexa_typetypecode"] = new OptionSetValue(createOrUpdatePortalConfigDto.Type);
                if (portalConfig != null)
                {
                    entity.Id = portalConfig.GetValueByAttributeName<Guid>("hexa_portalconfigurationid");
                    _crmService.GetInstance().Update(entity);
                }
                else
                {
                    _crmService.GetInstance().Create(entity);
                }

                await _cacheAppService.ClearEntityCache(EntityNames.Portalconfiguration);

                return createOrUpdatePortalConfigDto;
            }
            catch(Exception)
            {
                throw new UserFriendlyException("MsgUnexpectedError");
            }
            
        }
    }
}
