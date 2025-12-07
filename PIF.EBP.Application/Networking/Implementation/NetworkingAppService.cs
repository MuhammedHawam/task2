using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using PIF.EBP.Application.Networking;
using PIF.EBP.Application.Networking.DTOs;
using PIF.EBP.Application.MetaData.DTOs;
using PIF.EBP.Application.Shared;
using PIF.EBP.Application.Shared.Helpers;
using PIF.EBP.Core.CRM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static PIF.EBP.Application.Shared.Enums;
using Microsoft.Xrm.Sdk.Metadata;
using PIF.EBP.Core.PartnersHub.DTOs;
using PIF.EBP.Core.PartnersHub;

namespace PIF.EBP.Application.Networking.Implementation
{
    public class NetworkingAppService : INetworkingAppService
    {
        private readonly ICrmService _crmService;
        private readonly IPartnersHubService _partnersHubService;

        // Cached resolution of City -> Region attribute logical name
        private string _cityRegionAttributeLogicalName; // e.g. "ntw_regionid" or "pwc_regionid"
        private bool _cityRegionIsLookup; // whether the attribute is a lookup (required for Guid filters)

        public NetworkingAppService(ICrmService crmService, IPartnersHubService partnersHubService)
        {
            _crmService = crmService;
            _partnersHubService = partnersHubService;
        }

        public async Task<NetworkingCompaniesResponseDto> RetrieveNetworkingCompanies(NetworkingCompaniesRequestDto request)
        {
            var response = new NetworkingCompaniesResponseDto
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            // Build query
            var query = BuildNetworkingCompaniesQuery(request);

            // Get total count for pagination
            response.TotalCount = GetNetworkingCompaniesTotalCount(request);
            response.TotalPages = (int)Math.Ceiling((double)response.TotalCount / request.PageSize);

            // Retrieve companies
            var entityCollection = _crmService.GetInstance().RetrieveMultiple(query);
            var companies = entityCollection.Entities.Select(entity => FillNetworkingCompany(entity)).ToList();

            // Apply search text filter (case-insensitive, matches name + tagline)
            if (!string.IsNullOrEmpty(request.SearchText))
            {
                companies = companies.Where(c =>
          (!string.IsNullOrEmpty(c.Name) && c.Name.IndexOf(request.SearchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
               (!string.IsNullOrEmpty(c.NameAr) && c.NameAr.IndexOf(request.SearchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
           (!string.IsNullOrEmpty(c.Tagline) && c.Tagline.IndexOf(request.SearchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
             (!string.IsNullOrEmpty(c.TaglineAr) && c.TaglineAr.IndexOf(request.SearchText, StringComparison.OrdinalIgnoreCase) >= 0)
                ).ToList();

                // Update total count after search filter
                response.TotalCount = companies.Count;
                response.TotalPages = (int)Math.Ceiling((double)response.TotalCount / request.PageSize);
            }

            // Get activity counts from PartnersHub API for all companies
            if (companies.Any())
            {
                var companyGuids = companies.Select(c => Guid.Parse(c.Id)).ToList();
                var activityCounts = await _partnersHubService.GetActivityCountsByCompanyIdsAsync(companyGuids);

                foreach (var company in companies)
                {
                    if (Guid.TryParse(company.Id, out Guid companyGuid) && activityCounts.ContainsKey(companyGuid))
                    {
                        var counts = activityCounts[companyGuid];
                        company.ChallengesCount = counts.ChallengesCount;
                        company.CampaignsCount = counts.CampaignsCount;
                        company.TotalActivity = counts.ChallengesCount + counts.CampaignsCount;
                    }
                }
            }

            // Apply sorting
            companies = ApplyNetworkingSorting(companies, request.SortBy);

            // Apply pagination after sorting
            companies = companies
                  .Skip((request.PageNumber - 1) * request.PageSize)
                   .Take(request.PageSize)
                   .ToList();

            response.Companies = companies;
            return response;
        }

        public async Task<NetworkingCompanyDetailsDto> GetNetworkingCompanyById(string companyId)
        {
            if (string.IsNullOrEmpty(companyId) || !Guid.TryParse(companyId, out Guid companyGuid))
            {
                throw new Core.Exceptions.UserFriendlyException("InvalidCompanyId");
            }


            var query = new QueryExpression(EntityNames.Account)
            {
                ColumnSet = new ColumnSet(
        "accountid",
              "name",
          "ntw_companynamearabic",
         "entityimage",
           "description",
         "ntw_descriptionar",
              "websiteurl",
             "ntw_establishmentdate",
               "createdon"
      ),
                Criteria = new FilterExpression
                {
                    FilterOperator = LogicalOperator.And,
                    Conditions =
    {
                      new ConditionExpression("accountid", ConditionOperator.Equal, companyGuid),
                      new ConditionExpression("statecode", ConditionOperator.Equal, 0),
                      new ConditionExpression("ntw_isitannounced", ConditionOperator.Equal, true)
 }
                },
                LinkEntities =
      {
  // Link to GICS Sector
      new LinkEntity
   {
        LinkFromEntityName = EntityNames.Account,
      LinkFromAttributeName = "ntw_gicssectorid",
      LinkToEntityName = EntityNames.GICSSector,
   LinkToAttributeName = "ntw_gicssectorid",
           JoinOperator = JoinOperator.LeftOuter,
      EntityAlias = "sector",
    Columns = new ColumnSet("ntw_gicssectorid", "ntw_name", "pwc_referenceidarabic")
   },
         // Link to City (ntw_cities via pwc_cityid)
    new LinkEntity
 {
      LinkFromEntityName = EntityNames.Account,
  LinkFromAttributeName = "pwc_cityid",
      LinkToEntityName = EntityNames.City,
 LinkToAttributeName = "ntw_citiesid",
 JoinOperator = JoinOperator.LeftOuter,
       EntityAlias = "city",
          Columns = BuildCityColumnSet()
   },
       // Link to Point of Contact
         new LinkEntity
           {
         LinkFromEntityName = EntityNames.Account,
   LinkFromAttributeName = "ntw_pointofcontactid",
     LinkToEntityName = EntityNames.Contact,
              LinkToAttributeName = "contactid",
         JoinOperator = JoinOperator.LeftOuter,
      EntityAlias = "pointOfContact",
 Columns = new ColumnSet("entityimage", "firstname", "ntw_firstnamearabic", "lastname", "ntw_lastnamearabic", "address1_country","emailaddress1", "telephone1", "mobilephone", "pwc_position")
     }
}
            };

            // Nest link to Position under the pointOfContact link (contact -> position)
            var pointOfContactLink = query.LinkEntities.FirstOrDefault(le => le.EntityAlias == "pointOfContact");
            if (pointOfContactLink != null)
            {
                pointOfContactLink.LinkEntities.Add(new LinkEntity
                {
                    LinkFromEntityName = EntityNames.Contact,
                    LinkFromAttributeName = "pwc_position", // correct lookup on contact
                    LinkToEntityName = EntityNames.Position,
                    LinkToAttributeName = "ntw_positionid",
                    JoinOperator = JoinOperator.LeftOuter,
                    EntityAlias = "position",
                    Columns = new ColumnSet("ntw_name", "ntw_namear")
                });
            }

            var entityCollection = _crmService.GetInstance().RetrieveMultiple(query);

            if (entityCollection.Entities.Count == 0)
            {
                throw new Core.Exceptions.UserFriendlyException("CompanyNotFound");
            }

            var entity = entityCollection.Entities.FirstOrDefault();
            var companyDetails = FillNetworkingCompanyDetails(entity);

            // Get full challenges and campaigns data from PartnersHub API
            try
            {
                var challengesAndCampaigns = await _partnersHubService.GetChallengesByCompanyIdAsync(
                    new List<Guid> { companyGuid },
                    pageNumber: 1,
                    pageSize: 1000); // Get all data

                if (challengesAndCampaigns != null)
                {
                    // Populate challenges
                    companyDetails.Challenges = challengesAndCampaigns.Challenges?.Items ?? new List<ChallengeCompanyDTO>();
                    companyDetails.ChallengesCount = challengesAndCampaigns.Challenges?.TotalCount ?? 0;

                    // Populate campaigns
                    companyDetails.Campaigns = challengesAndCampaigns.Campaigns?.Items ?? new List<CampaignCompanyDTO>();
                    companyDetails.CampaignsCount = challengesAndCampaigns.Campaigns?.TotalCount ?? 0;

                    // Calculate total activity
                    companyDetails.TotalActivity = companyDetails.ChallengesCount + companyDetails.CampaignsCount;
                }
                else
                {
                    companyDetails.Challenges = new List<ChallengeCompanyDTO>();
                    companyDetails.Campaigns = new List<CampaignCompanyDTO>();
                    companyDetails.ChallengesCount = 0;
                    companyDetails.CampaignsCount = 0;
                    companyDetails.TotalActivity = 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error getting challenges and campaigns for company {companyGuid}: {ex.Message}");

                // Initialize empty lists on error
                companyDetails.Challenges = new List<ChallengeCompanyDTO>();
                companyDetails.Campaigns = new List<CampaignCompanyDTO>();
                companyDetails.ChallengesCount = 0;
                companyDetails.CampaignsCount = 0;
                companyDetails.TotalActivity = 0;
            }

            return companyDetails;
        }

        private NetworkingCompanyDetailsDto FillNetworkingCompanyDetails(Entity entity)
        {
            var details = new NetworkingCompanyDetailsDto
            {
                Id = entity.Id.ToString(),
                Name = CRMOperations.GetValueByAttributeName<string>(entity, "name"),
                NameAr = CRMOperations.GetValueByAttributeName<string>(entity, "ntw_companynamearabic"),
                Logo = CRMOperations.GetValueByAttributeName<byte[]>(entity, "entityimage"),

                // Location from linked city entity
                Location = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "city.ntw_name")?.Value?.ToString() ?? string.Empty,
                LocationAr = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "city.pwc_namear")?.Value?.ToString() ?? string.Empty,

                // Sector from linked GICS Sector
                Sector = new EntityReferenceDto
                {
                    Id = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "sector.ntw_gicssectorid")?.Value?.ToString(),
                    Name = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "sector.ntw_name")?.Value?.ToString(),
                    NameAr = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "sector.pwc_referenceidarabic")?.Value?.ToString()
                },

                // Full description (not truncated)
                Description = CRMOperations.GetValueByAttributeName<string>(entity, "description"),
                DescriptionAr = CRMOperations.GetValueByAttributeName<string>(entity, "ntw_descriptionar"),

                // Additional details
                Website = CRMOperations.GetValueByAttributeName<string>(entity, "websiteurl"),
                EstablishmentDate = CRMOperations.GetValueByAttributeName<DateTime?>(entity, "ntw_establishmentdate"),
                CreatedOn = CRMOperations.GetValueByAttributeName<DateTime?>(entity, "createdon"),

                // Industry - use sector name as industry
                Industry = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "sector.ntw_name")?.Value?.ToString() ?? string.Empty,
                IndustryAr = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "sector.pwc_referenceidarabic")?.Value?.ToString() ?? string.Empty
            };

            // Fill Representative Information (Point of Contact)
            var pointOfContactImage = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "pointOfContact.entityimage")?.Value;
            var firstName = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "pointOfContact.firstname")?.Value?.ToString() ?? string.Empty;
            var lastName = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "pointOfContact.lastname")?.Value?.ToString() ?? string.Empty;
            var firstNameAr = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "pointOfContact.ntw_firstnamearabic")?.Value?.ToString() ?? string.Empty;
            var lastNameAr = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "pointOfContact.ntw_lastnamearabic")?.Value?.ToString() ?? string.Empty;

            details.Representative = new RepresentativeInfo
            {
                Name = $"{firstName} {lastName}".Trim(),
                NameAr = $"{firstNameAr} {lastNameAr}".Trim(),
                Email = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "pointOfContact.emailaddress1")?.Value?.ToString() ?? string.Empty,
                Phone = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "pointOfContact.telephone1")?.Value?.ToString() ?? string.Empty,
                Mobile = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "pointOfContact.mobilephone")?.Value?.ToString() ?? string.Empty,
                Photo = pointOfContactImage != null ? (byte[])pointOfContactImage : null,
                Position = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "position.ntw_name")?.Value?.ToString() ?? string.Empty,
                PositionAr = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "position.ntw_namear")?.Value?.ToString() ?? string.Empty
            };

            return details;
        }

        private QueryExpression BuildNetworkingCompaniesQuery(NetworkingCompaniesRequestDto request)
        {

            var query = new QueryExpression(EntityNames.Account)
            {
                ColumnSet = new ColumnSet(
                  "accountid",      // Id
                     "name",    // Name
                    "ntw_companynamearabic",  // NameAr
                     "entityimage",  // Logo
                 "description",      // Tagline (English)
             "ntw_descriptionar", // Tagline (Arabic)
                 "createdon"     // CreatedOn
                  ),
                Criteria = new FilterExpression
                {
                    FilterOperator = LogicalOperator.And,
                    Conditions =
    {
             // Only active companies
          new ConditionExpression("statecode", ConditionOperator.Equal, 0),
        
        // Only published/announced companies
            new ConditionExpression("ntw_isitannounced", ConditionOperator.Equal, true)
    }
                },
                LinkEntities =
  {
        // Link to GICS Sector
        new LinkEntity
        {
               LinkFromEntityName = EntityNames.Account,
         LinkFromAttributeName = "ntw_gicssectorid",
      LinkToEntityName = EntityNames.GICSSector,
                  LinkToAttributeName = "ntw_gicssectorid",
          JoinOperator = JoinOperator.LeftOuter,
                EntityAlias = "sector",
         Columns = new ColumnSet("ntw_gicssectorid", "ntw_name", "pwc_referenceidarabic")
        },
   
              // Link to City (ntw_cities via pwc_cityid)
    new LinkEntity
           {
    LinkFromEntityName = EntityNames.Account,
      LinkFromAttributeName = "pwc_cityid",
        LinkToEntityName = EntityNames.City,
LinkToAttributeName = "ntw_citiesid",
              JoinOperator = JoinOperator.LeftOuter,
      EntityAlias = "city",
            Columns = BuildCityColumnSet(),
 LinkCriteria = BuildCityLinkFilter(request)
       }
     }
            };

            // Apply sector filter (multi-select)
            if (request.SectorIds != null && request.SectorIds.Any())
            {
                query.Criteria.AddCondition("ntw_gicssectorid", ConditionOperator.In,
                 request.SectorIds.Cast<object>().ToArray());
            }

            return query;
        }

        private int GetNetworkingCompaniesTotalCount(NetworkingCompaniesRequestDto request)
        {

            // Build QueryExpression for count

            //var  test= _crmService.GetMetaData(EntityNames.Account).Attributes.Select(f=>f.LogicalName).ToArray();
            //string jsonString = JsonConvert.SerializeObject(test);
            var countQuery = new QueryExpression(EntityNames.Account)
            {
                ColumnSet = new ColumnSet(false), // No columns needed for count
                Criteria = new FilterExpression
                {
                    FilterOperator = LogicalOperator.And,
                    Conditions =
   {
 new ConditionExpression("statecode", ConditionOperator.Equal, 0),
          new ConditionExpression("ntw_isitannounced", ConditionOperator.Equal, true)
            }
                }
            };

            // Apply sector filter
            if (request.SectorIds != null && request.SectorIds.Any())
            {
                countQuery.Criteria.AddCondition("ntw_gicssectorid", ConditionOperator.In,
                   request.SectorIds.Cast<object>().ToArray());
            }

            // Apply city filter
            if (request.CityIds != null && request.CityIds.Any())
            {
                countQuery.Criteria.AddCondition("pwc_cityid", ConditionOperator.In,
             request.CityIds.Cast<object>().ToArray());
            }

            // Apply region filter - need to link to city entity to filter by region
            if (request.RegionIds != null && request.RegionIds.Any() && !string.IsNullOrEmpty(_cityRegionAttributeLogicalName))
            {
                // Only apply the region filter if the region attribute is a lookup (RegionIds are Guids)
                if (_cityRegionIsLookup)
                {
                    var cityLink = new LinkEntity
                    {
                        LinkFromEntityName = EntityNames.Account,
                        LinkFromAttributeName = "pwc_cityid",
                        LinkToEntityName = EntityNames.City,
                        LinkToAttributeName = "ntw_citiesid",
                        JoinOperator = JoinOperator.Inner,
                        LinkCriteria = new FilterExpression
                        {
                            FilterOperator = LogicalOperator.And
                        }
                    };

                    cityLink.LinkCriteria.AddCondition(_cityRegionAttributeLogicalName, ConditionOperator.In,
                   request.RegionIds.Cast<object>().ToArray());

                    countQuery.LinkEntities.Add(cityLink);
                }
            }

            // Execute query and get count
            var result = _crmService.GetInstance().RetrieveMultiple(countQuery);
            return result.Entities.Count;
        }

        private NetworkingCompanyDto FillNetworkingCompany(Entity entity)
        {
            string regionName = string.Empty;
            if (!string.IsNullOrEmpty(_cityRegionAttributeLogicalName))
            {
                var regionAliasKey = "city." + _cityRegionAttributeLogicalName;
                if (entity.FormattedValues != null && entity.FormattedValues.ContainsKey(regionAliasKey))
                {
                    regionName = entity.FormattedValues[regionAliasKey];
                }
                else
                {
                    var aliased = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, regionAliasKey);
                    var value = aliased != null ? aliased.Value : null;
                    var er = value as EntityReference;
                    if (er != null)
                        regionName = er.Name ?? string.Empty;
                    else if (value != null)
                        regionName = value.ToString();
                }
            }

            var company = new NetworkingCompanyDto
            {
                Id = entity.Id.ToString(),
                Name = CRMOperations.GetValueByAttributeName<string>(entity, "name"),
                NameAr = CRMOperations.GetValueByAttributeName<string>(entity, "ntw_companynamearabic"),
                Logo = CRMOperations.GetValueByAttributeName<byte[]>(entity, "entityimage"),

                // City and Region Name from linked city entity
                City = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "city.ntw_name")?.Value?.ToString() ?? string.Empty,
                RegionName = regionName,

                // Sector from linked GICS Sector
                Sector = new EntityReferenceDto
                {
                    Id = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "sector.ntw_gicssectorid")?.Value?.ToString(),
                    Name = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "sector.ntw_name")?.Value?.ToString(),
                    NameAr = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "sector.pwc_referenceidarabic")?.Value?.ToString()
                },

                // Tagline (truncate to150 chars)
                Tagline = TruncateText(CRMOperations.GetValueByAttributeName<string>(entity, "description"), 150),
                TaglineAr = TruncateText(CRMOperations.GetValueByAttributeName<string>(entity, "ntw_descriptionar"), 150),

                // Created date for Newest sorting
                CreatedOn = CRMOperations.GetValueByAttributeName<DateTime?>(entity, "createdon"),

                // Activity counts will be populated later from PartnersHub API
                ChallengesCount = 0,
                CampaignsCount = 0,
                TotalActivity = 0
            };

            return company;
        }

        private string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text;

            return text.Substring(0, maxLength) + "...";
        }

        /// <summary>
        /// Get activity counts from PartnersHub API for a single company
        /// </summary>
        private async Task<(int ChallengesCount, int CampaignsCount)> GetCompanyActivityCountsAsync(Guid companyId)
        {
            try
            {
                var counts = await _partnersHubService.GetActivityCountsByCompanyIdsAsync(new List<Guid> { companyId });
                if (counts != null && counts.ContainsKey(companyId))
                {
                    return counts[companyId];
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error getting activity counts for company {companyId}: {ex.Message}");
            }

            return (0, 0);
        }

        /// <summary>
        /// Legacy method kept for backward compatibility - now uses PartnersHub API
        /// </summary>
        private (int ChallengesCount, int CampaignsCount) GetCompanyActivityCounts(Guid companyId)
        {
            // Synchronous wrapper for backward compatibility
            return GetCompanyActivityCountsAsync(companyId).GetAwaiter().GetResult();
        }

        private List<NetworkingCompanyDto> ApplyNetworkingSorting(
 List<NetworkingCompanyDto> companies,
 NetworkingSortOrder sortBy)
        {
            switch (sortBy)
            {
                case NetworkingSortOrder.MostActive:
                    return companies.OrderByDescending(c => c.TotalActivity)
                    .ThenBy(c => c.Name)
                    .ToList();

                case NetworkingSortOrder.AlphabeticalAZ:
                    return companies.OrderBy(c => c.Name).ToList();

                case NetworkingSortOrder.Newest:
                    return companies.OrderByDescending(c => c.CreatedOn).ToList();

                case NetworkingSortOrder.Location:
                    return companies.OrderBy(c => c.City)
                    .ThenBy(c => c.Name)
                    .ToList();

                default:
                    return companies.OrderByDescending(c => c.TotalActivity).ToList();
            }
        }

        // Helper: resolve the region attribute on City entity once and cache
      

        // Helper: build City columns including region attribute if exists
        private ColumnSet BuildCityColumnSet()
        {
            if (!string.IsNullOrEmpty(_cityRegionAttributeLogicalName))
            {
                return new ColumnSet("ntw_name", "pwc_namear", _cityRegionAttributeLogicalName);
            }
            return new ColumnSet("ntw_name", "pwc_namear");
        }

        // Helper: build City link criteria for CityIds and RegionIds if applicable
        private FilterExpression BuildCityLinkFilter(NetworkingCompaniesRequestDto request)
        {
            var filter = new FilterExpression(LogicalOperator.And);

            if (request.CityIds != null && request.CityIds.Any())
            {
                filter.AddCondition("ntw_citiesid", ConditionOperator.In,
                    request.CityIds.Cast<object>().ToArray());
            }

            if (request.RegionIds != null && request.RegionIds.Any() && !string.IsNullOrEmpty(_cityRegionAttributeLogicalName) && _cityRegionIsLookup)
            {
                filter.AddCondition(_cityRegionAttributeLogicalName, ConditionOperator.In,
              request.RegionIds.Cast<object>().ToArray());
            }

            // If no conditions were added, return null so caller doesn't assign an empty filter
            if (!filter.Conditions.Any() && (filter.Filters == null || !filter.Filters.Any()))
            {
                return null;
            }

            return filter;
        }

        #region Networking Filters Methods

        /// <summary>
        /// Get all available filter options (cities, regions, sectors) for networking companies
        /// </summary>
        public async Task<NetworkingFiltersResponseDto> GetNetworkingFilters()
        {
            var response = new NetworkingFiltersResponseDto();

            // Fetch all filter options in parallel
            var citiesTask = GetNetworkingCities();
            var regionsTask = GetNetworkingRegions();
            var sectorsTask = GetNetworkingSectors();

            await Task.WhenAll(citiesTask, regionsTask, sectorsTask);

            response.Cities = citiesTask.Result;
            response.Regions = regionsTask.Result;
            response.Sectors = sectorsTask.Result;

            return response;
        }

        /// <summary>
        /// Get list of cities for networking company filters
        /// </summary>
        public Task<List<NetworkingCityDto>> GetNetworkingCities()
        {

            var columns = new List<string> { "ntw_citiesid", "ntw_name", "pwc_namear" };

            // Add region column if it exists
            if (!string.IsNullOrEmpty(_cityRegionAttributeLogicalName))
            {
                columns.Add(_cityRegionAttributeLogicalName);
            }

            var query = new QueryExpression(EntityNames.City)
            {
                ColumnSet = new ColumnSet(columns.ToArray()),
                Criteria = new FilterExpression
                {
                    FilterOperator = LogicalOperator.And,
                    Conditions =
   {
             new ConditionExpression("statecode", ConditionOperator.Equal, 0) // Active only
          }
                },
                Orders =
    {
         new OrderExpression("ntw_name", OrderType.Ascending)
        }
            };

            // If region is a lookup, link to region entity to get region names
            if (_cityRegionIsLookup && !string.IsNullOrEmpty(_cityRegionAttributeLogicalName))
            {
                // Try to determine region entity name (common patterns)
                var regionEntityName = "ntw_region"; // Default assumption

                var regionLink = new LinkEntity
                {
                    LinkFromEntityName = EntityNames.City,
                    LinkFromAttributeName = _cityRegionAttributeLogicalName,
                    LinkToEntityName = regionEntityName,
                    LinkToAttributeName = "ntw_regionid", // Assumed primary key
                    JoinOperator = JoinOperator.LeftOuter,
                    EntityAlias = "region",
                    Columns = new ColumnSet("ntw_name", "pwc_namear")
                };

                query.LinkEntities.Add(regionLink);
            }

            var entityCollection = _crmService.GetInstance().RetrieveMultiple(query);
            var cities = entityCollection.Entities.Select(entity => FillNetworkingCity(entity)).ToList();

            return Task.FromResult(cities);
        }

        /// <summary>
        /// Get list of regions for networking company filters
        /// </summary>
        public Task<List<NetworkingRegionDto>> GetNetworkingRegions()
        {
            var regions = new List<NetworkingRegionDto>();

            try
            {
                // Query the region entity directly
                var regionEntityName = "ntw_region"; // Common pattern for region entity

                // Get metadata to check which attributes exist
                EntityMetadata regionMetadata = null;
                try
                {
                    regionMetadata = _crmService.GetMetaData(regionEntityName);
                }
                catch
                {
                    // Region entity doesn't exist
                    System.Diagnostics.Trace.TraceWarning($"Region entity '{regionEntityName}' not found in CRM metadata");
                    return Task.FromResult(regions);
                }

                if (regionMetadata == null || regionMetadata.Attributes == null)
                {
                    return Task.FromResult(regions);
                }

                // Determine which columns to retrieve based on available attributes
                var columnsToRetrieve = new List<string>();

                // Primary ID is required
                var primaryIdAttribute = regionMetadata.PrimaryIdAttribute ?? "ntw_regionid";
                columnsToRetrieve.Add(primaryIdAttribute);

                // Check for name attribute (English)
                var nameAttribute = regionMetadata.Attributes.FirstOrDefault(a =>
        a.LogicalName.Equals("ntw_name", StringComparison.OrdinalIgnoreCase) ||
               a.LogicalName.Equals(regionMetadata.PrimaryNameAttribute, StringComparison.OrdinalIgnoreCase));

                string nameAttributeName = nameAttribute?.LogicalName ?? regionMetadata.PrimaryNameAttribute ?? "ntw_name";
                if (!columnsToRetrieve.Contains(nameAttributeName))
                {
                    columnsToRetrieve.Add(nameAttributeName);
                }

                // Check for Arabic name attribute (optional)
                string nameArAttributeName = null;
                var nameArAttribute = regionMetadata.Attributes.FirstOrDefault(a =>
                  a.LogicalName.Equals("ntw_namear", StringComparison.OrdinalIgnoreCase) ||
                          a.LogicalName.Equals("pwc_namear", StringComparison.OrdinalIgnoreCase) ||
                          a.LogicalName.Equals("ntw_arabicname", StringComparison.OrdinalIgnoreCase) ||
                          a.LogicalName.Equals("ntw_namearabic", StringComparison.OrdinalIgnoreCase) ||
                           a.LogicalName.Equals("pwc_arabicname", StringComparison.OrdinalIgnoreCase));
                var nameArAttribute2 = regionMetadata.Attributes.Select(d => d.LogicalName).ToList();


                if (nameArAttribute != null)
                {
                    nameArAttributeName = nameArAttribute.LogicalName;
                    var ds = "ntw_cityid";
                    columnsToRetrieve.Add(nameArAttributeName);
                    columnsToRetrieve.Add(ds);
                }

                var query = new QueryExpression(regionEntityName)
                {
                    ColumnSet = new ColumnSet(columnsToRetrieve.ToArray()),
                    Criteria = new FilterExpression
                    {
                        FilterOperator = LogicalOperator.And,
                    },
                    Orders =
          {
     new OrderExpression(nameAttributeName, OrderType.Ascending)
  }
                };

                var entityCollection = _crmService.GetInstance().RetrieveMultiple(query);
                var fdd = entityCollection.Entities.Select(f => f.Attributes).ToList();

                regions = entityCollection.Entities.Select(entity => new NetworkingRegionDto
                {
                    Id = entity.Id,
                    Name = CRMOperations.GetValueByAttributeName<string>(entity, nameAttributeName) ?? string.Empty,
                    NameAr = !string.IsNullOrEmpty(nameArAttributeName)
       ? CRMOperations.GetValueByAttributeName<string>(entity, nameArAttributeName) ?? string.Empty
           : string.Empty
                }).ToList();
            }
            catch (Exception ex)
            {
                // Log error and return empty list if region entity doesn't exist
                System.Diagnostics.Trace.TraceError($"Error retrieving regions: {ex.Message}");
            }

            return Task.FromResult(regions);
        }
        /// <summary>
        /// Get list of GICS sectors for networking company filters
        /// </summary>
        public Task<List<NetworkingSectorDto>> GetNetworkingSectors()
        {
            var query = new QueryExpression(EntityNames.GICSSector)
            {
                ColumnSet = new ColumnSet("ntw_gicssectorid", "ntw_name", "pwc_referenceidarabic"),
                Criteria = new FilterExpression
                {
                    FilterOperator = LogicalOperator.And,
                    Conditions =
       {
         new ConditionExpression("statecode", ConditionOperator.Equal, 0) // Active only
     }
                },
                Orders =
                {
           new OrderExpression("ntw_name", OrderType.Ascending)
         }
            };

            var entityCollection = _crmService.GetInstance().RetrieveMultiple(query);

            var sectors = entityCollection.Entities.Select(entity => new NetworkingSectorDto
            {
                Id = entity.Id,
                Name = CRMOperations.GetValueByAttributeName<string>(entity, "ntw_name") ?? string.Empty,
                NameAr = CRMOperations.GetValueByAttributeName<string>(entity, "pwc_referenceidarabic") ?? string.Empty
            }).ToList();

            return Task.FromResult(sectors);
        }

        /// <summary>
        /// Helper method to populate NetworkingCityDto from entity
        /// </summary>
        private NetworkingCityDto FillNetworkingCity(Entity entity)
        {
            var city = new NetworkingCityDto
            {
                Id = entity.Id,
                Name = CRMOperations.GetValueByAttributeName<string>(entity, "ntw_name") ?? string.Empty,
                NameAr = CRMOperations.GetValueByAttributeName<string>(entity, "pwc_namear") ?? string.Empty
            };

            return city;
        }

        #endregion
    }
}
