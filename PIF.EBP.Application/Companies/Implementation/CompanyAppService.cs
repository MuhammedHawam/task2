using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using PIF.EBP.Application.Companies.DTOs;
using PIF.EBP.Application.MetaData.DTOs;
using PIF.EBP.Application.Networking.DTOs;
using PIF.EBP.Application.Shared;
using PIF.EBP.Application.Shared.Helpers;
using PIF.EBP.Core.CRM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static PIF.EBP.Application.Shared.Enums;
using PIF.EBP.Core.PartnersHub.DTOs;
using PIF.EBP.Core.PartnersHub;

namespace PIF.EBP.Application.Companies.Implementation
{
    public class CompanyAppService : ICompanyAppService
    {
        private readonly ICrmService _crmService;
        private readonly IPartnersHubService _partnersHubService;

        // Cached resolution of City -> Region attribute logical name
        private string _cityRegionAttributeLogicalName;
        private bool _cityRegionIsLookup;

        public CompanyAppService(ICrmService crmService, IPartnersHubService partnersHubService)
        {
            _crmService = crmService;
            _partnersHubService = partnersHubService;
        }

        public async Task<CompanyResponseDto> GetCompanies(CompanyRequestDto request)
        {
            var response = new CompanyResponseDto
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            // Build query
            var query = BuildCompaniesQuery(request);

            // Get total count for pagination
            response.TotalCount = GetCompaniesTotalCount(request);
            response.TotalPages = (int)Math.Ceiling((double)response.TotalCount / request.PageSize);

            // Retrieve companies
            var entityCollection = _crmService.GetInstance().RetrieveMultiple(query);
            var companies = entityCollection.Entities.Select(entity => FillCompany(entity)).ToList();

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
                try
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
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceError($"Error getting activity counts: {ex.Message}");
                    // Continue without activity counts
                }
            }

            // Apply sorting
            companies = ApplySorting(companies, request.SortBy);

            // Apply pagination after sorting
            companies = companies
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            response.Companies = companies;
            return response;
        }

        public async Task<CompanyDetailsDto> GetCompanyById(string companyId)
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
                    // Link to City
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
                        Columns = new ColumnSet("entityimage", "firstname", "ntw_firstnamearabic", "lastname", "ntw_lastnamearabic", "address1_country", "emailaddress1", "telephone1", "mobilephone", "pwc_position")
                    }
                }
            };

            // Nest link to Position under the pointOfContact link
            var pointOfContactLink = query.LinkEntities.FirstOrDefault(le => le.EntityAlias == "pointOfContact");
            if (pointOfContactLink != null)
            {
                pointOfContactLink.LinkEntities.Add(new LinkEntity
                {
                    LinkFromEntityName = EntityNames.Contact,
                    LinkFromAttributeName = "pwc_position",
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
            var companyDetails = FillCompanyDetails(entity);

            // Get full challenges and campaigns data from PartnersHub API
            try
            {
                var challengesAndCampaigns = await _partnersHubService.GetChallengesByCompanyIdAsync(
                    new List<Guid> { companyGuid },
                    pageNumber: 1,
                    pageSize: 1000);

                if (challengesAndCampaigns != null)
                {
                    companyDetails.Challenges = challengesAndCampaigns.Challenges?.Items ?? new List<ChallengeCompanyDTO>();
                    companyDetails.ChallengesCount = challengesAndCampaigns.Challenges?.TotalCount ?? 0;

                    companyDetails.Campaigns = challengesAndCampaigns.Campaigns?.Items ?? new List<CampaignCompanyDTO>();
                    companyDetails.CampaignsCount = challengesAndCampaigns.Campaigns?.TotalCount ?? 0;

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

                companyDetails.Challenges = new List<ChallengeCompanyDTO>();
                companyDetails.Campaigns = new List<CampaignCompanyDTO>();
                companyDetails.ChallengesCount = 0;
                companyDetails.CampaignsCount = 0;
                companyDetails.TotalActivity = 0;
            }

            return companyDetails;
        }

        public Task<List<CompanySectorDto>> GetCompanySectors()
        {
            var query = new QueryExpression(EntityNames.GICSSector)
            {
                ColumnSet = new ColumnSet("ntw_gicssectorid", "ntw_name", "pwc_referenceidarabic"),
                Criteria = new FilterExpression
                {
                    FilterOperator = LogicalOperator.And,
                    Conditions =
                    {
                        new ConditionExpression("statecode", ConditionOperator.Equal, 0)
                    }
                },
                Orders =
                {
                    new OrderExpression("ntw_name", OrderType.Ascending)
                }
            };

            var entityCollection = _crmService.GetInstance().RetrieveMultiple(query);

            var sectors = entityCollection.Entities.Select(entity => new CompanySectorDto
            {
                Id = entity.Id,
                Name = CRMOperations.GetValueByAttributeName<string>(entity, "ntw_name") ?? string.Empty,
                NameAr = CRMOperations.GetValueByAttributeName<string>(entity, "pwc_referenceidarabic") ?? string.Empty
            }).ToList();

            return Task.FromResult(sectors);
        }

        #region Private Helper Methods

        private QueryExpression BuildCompaniesQuery(CompanyRequestDto request)
        {
            var query = new QueryExpression(EntityNames.Account)
            {
                ColumnSet = new ColumnSet(
                    "accountid",
                    "name",
                    "ntw_companynamearabic",
                    "entityimage",
                    "description",
                    "ntw_descriptionar",
                    "createdon"
                ),
                Criteria = new FilterExpression
                {
                    FilterOperator = LogicalOperator.And,
                    Conditions =
                    {
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
                    // Link to City
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

            // Apply sector filter
            if (request.SectorIds != null && request.SectorIds.Any())
            {
                query.Criteria.AddCondition("ntw_gicssectorid", ConditionOperator.In,
                    request.SectorIds.Cast<object>().ToArray());
            }

            return query;
        }

        private int GetCompaniesTotalCount(CompanyRequestDto request)
        {
            var countQuery = new QueryExpression(EntityNames.Account)
            {
                ColumnSet = new ColumnSet(false),
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

            // Apply region filter
            if (request.RegionIds != null && request.RegionIds.Any() && !string.IsNullOrEmpty(_cityRegionAttributeLogicalName))
            {
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

            var result = _crmService.GetInstance().RetrieveMultiple(countQuery);
            return result.Entities.Count;
        }

        private CompanyDto FillCompany(Entity entity)
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

            var company = new CompanyDto
            {
                Id = entity.Id.ToString(),
                Name = CRMOperations.GetValueByAttributeName<string>(entity, "name"),
                NameAr = CRMOperations.GetValueByAttributeName<string>(entity, "ntw_companynamearabic"),
                Logo = CRMOperations.GetValueByAttributeName<byte[]>(entity, "entityimage"),

                City = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "city.ntw_name")?.Value?.ToString() ?? string.Empty,
                RegionName = regionName,

                Sector = new EntityReferenceDto
                {
                    Id = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "sector.ntw_gicssectorid")?.Value?.ToString(),
                    Name = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "sector.ntw_name")?.Value?.ToString(),
                    NameAr = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "sector.pwc_referenceidarabic")?.Value?.ToString()
                },

                Tagline = TruncateText(CRMOperations.GetValueByAttributeName<string>(entity, "description"), 150),
                TaglineAr = TruncateText(CRMOperations.GetValueByAttributeName<string>(entity, "ntw_descriptionar"), 150),

                CreatedOn = CRMOperations.GetValueByAttributeName<DateTime?>(entity, "createdon"),

                ChallengesCount = 0,
                CampaignsCount = 0,
                TotalActivity = 0
            };

            return company;
        }

        private CompanyDetailsDto FillCompanyDetails(Entity entity)
        {
            var details = new CompanyDetailsDto
            {
                Id = entity.Id.ToString(),
                Name = CRMOperations.GetValueByAttributeName<string>(entity, "name"),
                NameAr = CRMOperations.GetValueByAttributeName<string>(entity, "ntw_companynamearabic"),
                Logo = CRMOperations.GetValueByAttributeName<byte[]>(entity, "entityimage"),

                Location = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "city.ntw_name")?.Value?.ToString() ?? string.Empty,
                LocationAr = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "city.pwc_namear")?.Value?.ToString() ?? string.Empty,

                Sector = new EntityReferenceDto
                {
                    Id = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "sector.ntw_gicssectorid")?.Value?.ToString(),
                    Name = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "sector.ntw_name")?.Value?.ToString(),
                    NameAr = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "sector.pwc_referenceidarabic")?.Value?.ToString()
                },

                Description = CRMOperations.GetValueByAttributeName<string>(entity, "description"),
                DescriptionAr = CRMOperations.GetValueByAttributeName<string>(entity, "ntw_descriptionar"),

                Website = CRMOperations.GetValueByAttributeName<string>(entity, "websiteurl"),
                EstablishmentDate = CRMOperations.GetValueByAttributeName<DateTime?>(entity, "ntw_establishmentdate"),
                CreatedOn = CRMOperations.GetValueByAttributeName<DateTime?>(entity, "createdon"),

                Industry = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "sector.ntw_name")?.Value?.ToString() ?? string.Empty,
                IndustryAr = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "sector.pwc_referenceidarabic")?.Value?.ToString() ?? string.Empty
            };

            // Fill Representative Information
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

        private List<CompanyDto> ApplySorting(List<CompanyDto> companies, CompanySortOrder sortBy)
        {
            switch (sortBy)
            {
                case CompanySortOrder.MostActive:
                    return companies.OrderByDescending(c => c.TotalActivity)
                        .ThenBy(c => c.Name)
                        .ToList();

                case CompanySortOrder.AlphabeticalAZ:
                    return companies.OrderBy(c => c.Name).ToList();

                case CompanySortOrder.Newest:
                    return companies.OrderByDescending(c => c.CreatedOn).ToList();

                case CompanySortOrder.Location:
                    return companies.OrderBy(c => c.City)
                        .ThenBy(c => c.Name)
                        .ToList();

                default:
                    return companies.OrderByDescending(c => c.TotalActivity).ToList();
            }
        }

        private string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text;

            return text.Substring(0, maxLength) + "...";
        }

        private ColumnSet BuildCityColumnSet()
        {
            if (!string.IsNullOrEmpty(_cityRegionAttributeLogicalName))
            {
                return new ColumnSet("ntw_name", "pwc_namear", _cityRegionAttributeLogicalName);
            }
            return new ColumnSet("ntw_name", "pwc_namear");
        }

        private FilterExpression BuildCityLinkFilter(CompanyRequestDto request)
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

            if (!filter.Conditions.Any() && (filter.Filters == null || !filter.Filters.Any()))
            {
                return null;
            }

            return filter;
        }

        #endregion
    }
}
