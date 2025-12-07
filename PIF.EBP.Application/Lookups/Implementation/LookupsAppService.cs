using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using PIF.EBP.Application.Shared.Helpers;
using PIF.EBP.Application.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static PIF.EBP.Application.Shared.EntityNames;
using PIF.EBP.Core.CRM;
using PIF.EBP.Application.Lookups.DTOs;
using PIF.EBP.Application.PortalConfiguration;
using PIF.EBP.Application.EntitiesCache;

namespace PIF.EBP.Application.Lookups.Implementation
{
    public class LookupsAppService : ILookupsAppService
    {
        private readonly ICrmService _crmService;
        private readonly IPortalConfigAppService _portalConfigAppService;
        private readonly IEntitiesCacheAppService _entitiesCacheAppService;

        public LookupsAppService(ICrmService crmService,
                                 IPortalConfigAppService portalConfigAppService,
                                 IEntitiesCacheAppService entitiesCacheAppService)
        {
            _crmService = crmService;
            _portalConfigAppService = portalConfigAppService;
            _entitiesCacheAppService= entitiesCacheAppService;
        }

        public async Task<List<MasterLookup>> RetrieveLookUpsData(LookupDataRequestDto lookupDataRequestDto)
        {
            List<MasterLookup> masterLookups = new List<MasterLookup>();
            if (lookupDataRequestDto?.keys == null) return masterLookups;

            foreach (var key in lookupDataRequestDto.keys)
            {
                if (LookupDictionaryKeys.LookupConstants.TryGetValue(key.ToLower(), out var lookup))
                {
                    MasterLookup masterLookup = new MasterLookup
                    {
                        key = key
                    };

                    masterLookup.Values.AddRange(GetLookupsValue(lookup[LookupDictionaryKeys.EntityName],
                        lookup[LookupDictionaryKeys.Id],
                        lookup[LookupDictionaryKeys.Name],
                        lookup[LookupDictionaryKeys.NameAr]));
                    masterLookups.Add(masterLookup);
                }

                else if (LookupDictionaryKeys.OptiosetConstants.TryGetValue(key.ToLower(), out var OptioSet))
                {
                    MasterLookup masterLookup = new MasterLookup
                    {
                        key = key
                    };
                    var result = _entitiesCacheAppService.RetrieveOptionSetCacheByKey(key);
                    if (result != null || result.Any())
                    {
                        masterLookup.Values.AddRange(result.Select(x => new LookupValue { Id = x.Value, Name = x.Name, NameAr = x.NameAr }).ToList());

                        if (key.Equals("feedbacktype", StringComparison.OrdinalIgnoreCase))
                        {
                            var configs = _portalConfigAppService.RetrievePortalConfiguration(new List<string> { PortalConfigurations.DisplayComplimentFeedbackOnPortal });
                            bool.TryParse(configs.SingleOrDefault(a => a.Key == PortalConfigurations.DisplayComplimentFeedbackOnPortal)?.Value, out bool displayCompliment);
                            if (!displayCompliment)
                            {
                                masterLookup.Values=masterLookup.Values.Where(x => x.Id !="2").ToList();
                            }
                        }
                    }
                    masterLookups.Add(masterLookup);
                }
            }
            return masterLookups;
        }

        private List<LookupValue> GetLookupsValue(string entityName, string primaryId, string name, string nameAr)
        {
            var masterData = new List<LookupValue>();
            var cols = new List<string> { primaryId, name, nameAr };

            if (entityName.Equals(EntityNames.Country, StringComparison.OrdinalIgnoreCase))
            {
                cols.Add("pwc_flag");
            }
            var query = new QueryExpression(entityName)
            {
                ColumnSet = new ColumnSet(cols.ToArray()),
                Criteria = new FilterExpression()
            };

            if (entityName.Equals(EntityNames.PortalRole, StringComparison.OrdinalIgnoreCase))
            {
                var orFilter = new FilterExpression(LogicalOperator.Or);
                orFilter.AddCondition("hexa_name", ConditionOperator.Equal, "PC Contributor");
                orFilter.AddCondition("hexa_name", ConditionOperator.Equal, "PC Viewer");
                query.Criteria.AddFilter(orFilter);
            }

            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0 /*Active*/);

            var entityCollection = _crmService.GetInstance().RetrieveMultiple(query);

            if (entityCollection.Entities.Any())
            {
                return entityCollection.Entities.Select(entityValue => FillLookupsValue(entityValue, primaryId, name, nameAr)).ToList();
            }

            return new List<LookupValue>();
        }

        private LookupValue FillLookupsValue(Entity entity, string primaryId, string name, string nameAr)
        {
            var lookupValue = new LookupValue
            {
                Id = CRMOperations.GetValueByAttributeName<Guid>(entity, primaryId).ToString(),
                Name = CRMOperations.GetValueByAttributeName<string>(entity, name),
                NameAr = CRMOperations.GetValueByAttributeName<string>(entity, nameAr)
            };

            if (entity.LogicalName.Equals(EntityNames.Country, StringComparison.OrdinalIgnoreCase))
            {
                lookupValue.CountryFlag = CRMOperations.GetValueByAttributeName<string>(entity, "pwc_flag");
            }

            return lookupValue;
        }

    }
}
