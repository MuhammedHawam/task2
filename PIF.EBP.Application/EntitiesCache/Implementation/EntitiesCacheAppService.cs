using PIF.EBP.Application.MetaData.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;
using PIF.EBP.Application.EntitiesCache.DTOs;
using PIF.EBP.Application.Shared;
using System;
using System.Linq;
using static PIF.EBP.Application.Shared.EntityNames;
using Microsoft.Xrm.Sdk;

namespace PIF.EBP.Application.EntitiesCache.Implementation
{
    public class EntitiesCacheAppService : IEntitiesCacheAppService
    {
        private readonly IEntitiesCacheManager _entitiesCacheManager;
        public EntitiesCacheAppService(IEntitiesCacheManager entitiesCacheManager)
        {
            _entitiesCacheManager = entitiesCacheManager;
        }

        public EntityReferenceDto RetrieveEntityCacheById(string entityName, Guid? entityId)
        {
            if (entityId == null || entityId == Guid.Empty)
            {
                return null;
            }
            if (entityId !=null && entityId !=Guid.Empty)
            {
                switch (entityName)
                {
                    case EntityNames.HexaStepStatusTemplate:
                        var cachedStepStatusItems = Task.Run(async () => await _entitiesCacheManager.GetCachedStepStatusTemplateAsync()).GetAwaiter().GetResult();
                        var stepStatusItem = cachedStepStatusItems?.Find(x => x.Id == entityId);
                        if (stepStatusItem != null)
                        {
                            return new EntityReferenceDto
                            {
                                Id = stepStatusItem.Id.ToString(),
                                Name = stepStatusItem.Name,
                                NameAr = stepStatusItem.NameAr
                            };
                        }
                        break;
                    case EntityNames.ProcessStatusTemplate:
                        var cachedProcessStatusItems = Task.Run(async () => await _entitiesCacheManager.GetCachedProcessStatusTemplateAsync()).GetAwaiter().GetResult();
                        var processStatusItem = cachedProcessStatusItems?.Find(x => x.Id == entityId);
                        if (processStatusItem != null)
                        {
                            return new EntityReferenceDto
                            {
                                Id = processStatusItem.Id.ToString(),
                                Name = processStatusItem.Name,
                                NameAr = processStatusItem.NameAr
                            };
                        }
                        break;
                    default:
                        break;
                }
            }
            return null;
        }

        public EntityOptionSetDto RetrieveOptionSetCacheByKeyWithValue(string key, OptionSetValue value)
        {
            if (string.IsNullOrEmpty(key) || value==null)
            {
                return null;
            }
            if (LookupDictionaryKeys.OptiosetConstants.TryGetValue(key.ToLower(), out var constant))
            {
                string entityName = constant[LookupDictionaryKeys.EntityName];
                string attributeName = constant[LookupDictionaryKeys.AttributeName];
                var cachedStepStatusItems = Task.Run(async () => await _entitiesCacheManager.GetCachedOptioSetsAsync()).GetAwaiter().GetResult();
                var optionSets = cachedStepStatusItems.First(x => x.EntityName==entityName && x.AttributeName==attributeName).OptionSets;
                return optionSets.First(z => z.Value==value.Value.ToString());
            }
            return null;
        }
        public List<EntityOptionSetDto> RetrieveOptionSetCacheByKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return new List<EntityOptionSetDto>();
            }
            if (LookupDictionaryKeys.OptiosetConstants.TryGetValue(key.ToLower(), out var constant))
            {
                string entityName = constant[LookupDictionaryKeys.EntityName];
                string attributeName = constant[LookupDictionaryKeys.AttributeName];
                var cachedStepStatusItems = Task.Run(async () => await _entitiesCacheManager.GetCachedOptioSetsAsync()).GetAwaiter().GetResult();
                var optionSets = cachedStepStatusItems.First(x => x.EntityName==entityName && x.AttributeName==attributeName).OptionSets;

                return optionSets;
            }
            return new List<EntityOptionSetDto>();
        }
        public async Task<List<PortalConfigDto>> RetrievePortalconfigurations()
        {
            var cachedItems = await _entitiesCacheManager.GetCachedPortalConfigsAsync();

            return cachedItems;
        }
        public async Task<List<StepStatusTemplateDto>> RetrieveStepStatusTemplates()
        {
            var cachedItems = await _entitiesCacheManager.GetCachedStepStatusTemplateAsync();

            return cachedItems;
        }
        public async Task<List<ProcessStatusTemplateDto>> RetrieveProcessStatusTemplates()
        {
            var cachedItems = await _entitiesCacheManager.GetCachedProcessStatusTemplateAsync();

            return cachedItems;
        }
        public async Task<List<OptionSetCache>> RetrieveCachedOptioSets()
        {
            var cachedItems = await _entitiesCacheManager.GetCachedOptioSetsAsync();

            return cachedItems;
        }
        
    }
}
