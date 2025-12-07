using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using PIF.EBP.Application.EntitiesCache.DTOs;
using PIF.EBP.Application.MetaData.DTOs;
using PIF.EBP.Application.Shared;
using PIF.EBP.Application.Shared.Helpers;
using PIF.EBP.Core.CRM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static PIF.EBP.Application.Shared.EntityNames;

namespace PIF.EBP.Application.EntitiesCache.Implementation
{
    public class EntitiesCrmQueries : IEntitiesCrmQueries
    {
        private readonly ICrmService _crmService;
        public EntitiesCrmQueries(ICrmService crmService)
        {
            _crmService = crmService;
        }
        public async Task<IEnumerable<TCacheItem>> GetItemFromDataSource<TCacheItem, TPrimaryKey>(string entityName)
            where TCacheItem : CacheItemBase<TPrimaryKey>
        {
            List<StepStatusTemplateDto> stepStatusTemplateList = null;
            List<PortalConfigDto> portalConfigsList = null;
            List<ProcessStatusTemplateDto> processStatusTemplateList = null;
            List<OptionSetCache> optionSetCacheList = null;

            switch (entityName)
            {
                case EntityNames.Portalconfiguration:
                    portalConfigsList = GetPortalConfigurations();
                    break;
                case EntityNames.HexaStepStatusTemplate:
                    stepStatusTemplateList = GetStepStatusTemplates();
                    break;
                case EntityNames.ProcessStatusTemplate:
                    processStatusTemplateList = GetProcessStatusTemplates();
                    break;
                case EntityNames.OptionSet:
                    optionSetCacheList = GetOptionSetsList();
                    break;
                default:
                    throw new ArgumentException($"Unknown entity name: {entityName}", nameof(entityName));
            }
            var cacheItem = new EntitiesCacheItem
            {
                Id = string.Empty,
                PortalConfigsList = portalConfigsList ?? new List<PortalConfigDto>(),
                StepStatusTemplateList = stepStatusTemplateList ?? new List<StepStatusTemplateDto>(),
                ProcessStatusTemplateList = processStatusTemplateList ?? new List<ProcessStatusTemplateDto>(),
                OptionSetList = optionSetCacheList ?? new List<OptionSetCache>(),
            };
            return (IEnumerable<TCacheItem>)new List<EntitiesCacheItem>() { cacheItem };
        }
        public async Task<List<T>> GetFromDataSource<T>(string entityName) where T : ICacheItem
        {
            IEnumerable<ICacheItem> items;

            switch (entityName)
            {
                case EntityNames.HexaStepStatusTemplate when typeof(T) == typeof(StepStatusTemplateDto):
                    items = GetStepStatusTemplates();
                    break;
                case EntityNames.Portalconfiguration when typeof(T) == typeof(PortalConfigDto):
                    items = GetPortalConfigurations();
                    break;
                case EntityNames.ProcessStatusTemplate when typeof(T) == typeof(ProcessStatusTemplateDto):
                    items = GetProcessStatusTemplates();
                    break;
                case EntityNames.OptionSet when typeof(T) == typeof(OptionSetCache):
                    items = GetOptionSetsList();
                    break;
                default:
                    items = Enumerable.Empty<ICacheItem>();
                    break;
            }

            return items.OfType<T>().ToList();
        }
        private List<StepStatusTemplateDto> GetStepStatusTemplates()
        {
            var query = new QueryExpression(EntityNames.HexaStepStatusTemplate)
            {
                ColumnSet = new ColumnSet("hexa_stepstatustemplateid", "hexa_nameen", "hexa_namear", "hexa_type")
            };

            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0 /*Active*/);

            var entityCollection = _crmService.GetInstance().RetrieveMultiple(query);

            if (entityCollection.Entities.Any())
            {
                return entityCollection.Entities.Select(entityValue => FillStepStatusTemplates(entityValue)).ToList();
            }

            return new List<StepStatusTemplateDto>();
        }
        private StepStatusTemplateDto FillStepStatusTemplates(Entity entity)
        {
            return new StepStatusTemplateDto
            {
                Id=entity.Id,
                Name= entity.GetValueByAttributeName<string>("hexa_nameen"),
                NameAr= entity.GetValueByAttributeName<string>("hexa_namear"),
                Type=entity.GetValueByAttributeName<EntityOptionSetDto>("hexa_type"),
            };
        }

        private List<PortalConfigDto> GetPortalConfigurations()
        {
            var query = new QueryExpression(EntityNames.Portalconfiguration)
            {
                ColumnSet = new ColumnSet("hexa_portalconfigurationid", "hexa_name", "hexa_value", "pwc_valuear", "hexa_typetypecode", "statecode"),
                Criteria = new FilterExpression()
            };
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);//Active Status
            var entityCollection = _crmService.GetInstance().RetrieveMultiple(query);

            if (entityCollection.Entities.Any())
            {
                return entityCollection.Entities.Select(entityValue => FillPortalConfig(entityValue)).ToList();
            }

            return new List<PortalConfigDto>();
        }
        private PortalConfigDto FillPortalConfig(Entity entity)
        {
            return new PortalConfigDto
            {
                Key = CRMOperations.GetValueByAttributeName<string>(entity, "hexa_name"),
                Value = CRMOperations.GetValueByAttributeName<string>(entity, "hexa_value"),
                ValueAr = CRMOperations.GetValueByAttributeName<string>(entity, "pwc_valuear"),
                Type = CRMOperations.GetValueByAttributeName<EntityOptionSetDto>(entity, "hexa_typetypecode")
            };
        }

        private List<ProcessStatusTemplateDto> GetProcessStatusTemplates()
        {
            var query = new QueryExpression(EntityNames.ProcessStatusTemplate)
            {
                ColumnSet = new ColumnSet("hexa_processstatustemplateid", "hexa_nameen", "hexa_namear", "hexa_type")
            };

            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0 /*Active*/);

            var entityCollection = _crmService.GetInstance().RetrieveMultiple(query);

            if (entityCollection.Entities.Any())
            {
                return entityCollection.Entities.Select(entityValue => FillProcessStatusTemplates(entityValue)).ToList();
            }

            return new List<ProcessStatusTemplateDto>();
        }
        private ProcessStatusTemplateDto FillProcessStatusTemplates(Entity entity)
        {
            return new ProcessStatusTemplateDto
            {
                Id=entity.Id,
                Name= entity.GetValueByAttributeName<string>("hexa_nameen"),
                NameAr= entity.GetValueByAttributeName<string>("hexa_namear"),
                Type=entity.GetValueByAttributeName<EntityOptionSetDto>("hexa_type"),
            };
        }

        private List<OptionSetCache> GetOptionSetsList()
        {
            List<OptionSetCache> optionSetCaches = new List<OptionSetCache>();
            foreach (var option in LookupDictionaryKeys.OptiosetConstants)
            {
                OptionSetCache optionSetCache = new OptionSetCache();
                string entityName = option.Value[LookupDictionaryKeys.EntityName];
                string attributeName = option.Value[LookupDictionaryKeys.AttributeName];
                EntityOptionSetOptions oEntityOptionSetOptions = new EntityOptionSetOptions { EntityName = entityName, AttributeName = attributeName };
                var result = GetEntityOptionsSetValues(oEntityOptionSetOptions);
                if (result != null || result.Any())
                {
                    optionSetCache.AttributeName=attributeName;
                    optionSetCache.EntityName=entityName;
                    optionSetCache.OptionSets=result;
                    optionSetCaches.Add(optionSetCache);
                }

            }
            return optionSetCaches;
        }
        private  List<EntityOptionSetDto> GetEntityOptionsSetValues(EntityOptionSetOptions options)
        {
                var orgContext = new OrganizationServiceContext(_crmService.GetInstance());
                var optionSetValues = new List<EntityOptionSetDto>();

                var retrieveAttributeRequest = new RetrieveAttributeRequest
                {
                    EntityLogicalName = options.EntityName,
                    LogicalName = options.AttributeName,
                    RetrieveAsIfPublished = true
                    /*
                     RetrieveAsIfPublished = “True” means return the metadata(Entity, Attributes, Relationship, Optionset, option) including unpublished changes.
                     RetrieveAsIfPublished = “False” means return only the currently published Metadata, ignoring any unpublished changes.
                     */
                };

                AttributeMetadata attributeMetadata;

                var response = (RetrieveAttributeResponse)orgContext.Execute(retrieveAttributeRequest);
                attributeMetadata = response.AttributeMetadata;

                OptionMetadata[] optionList = null;
                if (attributeMetadata is StatusAttributeMetadata statusMetadata)
                {
                    optionList = statusMetadata.OptionSet.Options.ToArray();
                }
                else if (attributeMetadata is StateAttributeMetadata stateMetadata)
                {
                    optionList = stateMetadata.OptionSet.Options.ToArray();
                }
                else if (attributeMetadata is MultiSelectPicklistAttributeMetadata multiSelectMetadata)
                {
                    optionList = multiSelectMetadata.OptionSet.Options.ToArray();
                }
                else if (attributeMetadata is PicklistAttributeMetadata picklistMetadata)
                {
                    optionList = picklistMetadata.OptionSet.Options.ToArray();
                }

                if (optionList != null)
                {
                    int cultureCode_en = CRMUtility.GetCultureForCRM(Constants.LangEn);
                    int cultureCode_ar = CRMUtility.GetCultureForCRM(Constants.LangAr);
                    foreach (OptionMetadata option in optionList)
                    {
                        var localizedLabel = option.Label.LocalizedLabels.FirstOrDefault(x => x.LanguageCode == cultureCode_en)?.Label ??
                                             option.Label.LocalizedLabels.FirstOrDefault()?.Label;
                        var localizedLabel_ar = option.Label.LocalizedLabels.FirstOrDefault(x => x.LanguageCode == cultureCode_ar)?.Label ??
                                             option.Label.LocalizedLabels.FirstOrDefault()?.Label;

                        if (option.Value.HasValue && !string.IsNullOrWhiteSpace(localizedLabel))
                        {
                            optionSetValues.Add(new EntityOptionSetDto
                            {
                                Name = localizedLabel,
                                NameAr = localizedLabel_ar,
                                Value = option.Value.Value.ToString()
                            });
                        }
                    }
                }

                return optionSetValues.OrderBy(z => z.Name).ToList();
        }

        
    }
}
