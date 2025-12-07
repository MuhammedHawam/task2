using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using PIF.EBP.Application.Companies.DTOs;
using PIF.EBP.Application.Shared;
using PIF.EBP.Application.Shared.Helpers;
using PIF.EBP.Core.CRM;
using PIF.EBP.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PIF.EBP.Application.Companies.Implementation
{
    public class CompanyIntegrationAppService : ICompanyIntegrationAppService
    {
        private readonly ICrmService _crmService;

        public CompanyIntegrationAppService(ICrmService crmService)
        {
            _crmService = crmService ?? throw new ArgumentNullException(nameof(crmService));
        }

        public async Task<CompanyIntegrationResponseDto> GetCompanies(CompanyIntegrationRequestDto request)
        {
            var query = BuildQuery(request);
            var entityCollection = _crmService.GetInstance().RetrieveMultiple(query);

            var companies = entityCollection.Entities
                .Select(entity => MapToDto(entity))
                .ToList();

            // Apply search filter if provided
            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                companies = companies.Where(c =>
                    (!string.IsNullOrEmpty(c.Name) && c.Name.IndexOf(request.SearchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (!string.IsNullOrEmpty(c.NameAr) && c.NameAr.IndexOf(request.SearchText, StringComparison.OrdinalIgnoreCase) >= 0)
                ).ToList();
            }

            var totalCount = companies.Count;
            var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

            // Apply pagination
            companies = companies
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return await Task.FromResult(new CompanyIntegrationResponseDto
            {
                Companies = companies,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = totalPages
            });
        }

        public async Task<CompanyIntegrationDto> GetCompanyById(Guid companyId)
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
                    "websiteurl",
                    "ntw_establishmentdate",
                    "createdon",
                    "address1_country"
                ),
                Criteria = new FilterExpression
                {
                    FilterOperator = LogicalOperator.And,
                    Conditions =
                    {
                        new ConditionExpression("accountid", ConditionOperator.Equal, companyId),
                        new ConditionExpression("statecode", ConditionOperator.Equal, 0),
                        new ConditionExpression("ntw_isitannounced", ConditionOperator.Equal, true)
                    }
                },
                LinkEntities =
                {
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
                    new LinkEntity
                    {
                        LinkFromEntityName = EntityNames.Account,
                        LinkFromAttributeName = "pwc_cityid",
                        LinkToEntityName = EntityNames.City,
                        LinkToAttributeName = "ntw_citiesid",
                        JoinOperator = JoinOperator.LeftOuter,
                        EntityAlias = "city",
                        Columns = new ColumnSet("ntw_citiesid", "ntw_name", "pwc_namear")
                    },
                    new LinkEntity
                    {
                        LinkFromEntityName = EntityNames.Account,
                        LinkFromAttributeName = "ntw_pointofcontactid",
                        LinkToEntityName = EntityNames.Contact,
                        LinkToAttributeName = "contactid",
                        JoinOperator = JoinOperator.LeftOuter,
                        EntityAlias = "representative",
                        Columns = new ColumnSet("firstname", "lastname", "ntw_firstnamearabic", "ntw_lastnamearabic", "emailaddress1", "telephone1", "mobilephone", "pwc_position")
                    }
                }
            };

            // Add position link
            var representativeLink = query.LinkEntities.FirstOrDefault(le => le.EntityAlias == "representative");
            if (representativeLink != null)
            {
                representativeLink.LinkEntities.Add(new LinkEntity
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
                throw new UserFriendlyException("CompanyNotFound");
            }

            return await Task.FromResult(MapToDto(entityCollection.Entities.First()));
        }

        public async Task<List<CompanySectorDto>> GetSectors()
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

            return await Task.FromResult(sectors);
        }

        #region Private Methods

        private QueryExpression BuildQuery(CompanyIntegrationRequestDto request)
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
                    "websiteurl",
                    "ntw_establishmentdate",
                    "createdon",
                    "address1_country"
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
                    new LinkEntity
                    {
                        LinkFromEntityName = EntityNames.Account,
                        LinkFromAttributeName = "pwc_cityid",
                        LinkToEntityName = EntityNames.City,
                        LinkToAttributeName = "ntw_citiesid",
                        JoinOperator = JoinOperator.LeftOuter,
                        EntityAlias = "city",
                        Columns = new ColumnSet("ntw_citiesid", "ntw_name", "pwc_namear")
                    },
                    new LinkEntity
                    {
                        LinkFromEntityName = EntityNames.Account,
                        LinkFromAttributeName = "ntw_pointofcontactid",
                        LinkToEntityName = EntityNames.Contact,
                        LinkToAttributeName = "contactid",
                        JoinOperator = JoinOperator.LeftOuter,
                        EntityAlias = "representative",
                        Columns = new ColumnSet("firstname", "lastname", "emailaddress1", "telephone1", "mobilephone", "pwc_position")
                    }
                }
            };

            // Add position link
            var representativeLink = query.LinkEntities.FirstOrDefault(le => le.EntityAlias == "representative");
            if (representativeLink != null)
            {
                representativeLink.LinkEntities.Add(new LinkEntity
                {
                    LinkFromEntityName = EntityNames.Contact,
                    LinkFromAttributeName = "pwc_position",
                    LinkToEntityName = EntityNames.Position,
                    LinkToAttributeName = "ntw_positionid",
                    JoinOperator = JoinOperator.LeftOuter,
                    EntityAlias = "position",
                    Columns = new ColumnSet("ntw_name")
                });
            }

            // Apply sector filter
            if (request.SectorIds != null && request.SectorIds.Any())
            {
                query.Criteria.AddCondition("ntw_gicssectorid", ConditionOperator.In,
                    request.SectorIds.Cast<object>().ToArray());
            }

            // Apply city filter
            if (request.CityIds != null && request.CityIds.Any())
            {
                query.Criteria.AddCondition("pwc_cityid", ConditionOperator.In,
                    request.CityIds.Cast<object>().ToArray());
            }

            query.Orders.Add(new OrderExpression("name", OrderType.Ascending));

            return query;
        }

        private CompanyIntegrationDto MapToDto(Entity entity)
        {
            var logoBytes = CRMOperations.GetValueByAttributeName<byte[]>(entity, "entityimage");
            
            var firstName = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "representative.firstname")?.Value?.ToString() ?? string.Empty;
            var lastName = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "representative.lastname")?.Value?.ToString() ?? string.Empty;
            var firstNameAr = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "representative.ntw_firstnamearabic")?.Value?.ToString() ?? string.Empty;
            var lastNameAr = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "representative.ntw_lastnamearabic")?.Value?.ToString() ?? string.Empty;

            return new CompanyIntegrationDto
            {
                Id = entity.Id.ToString(),
                Name = CRMOperations.GetValueByAttributeName<string>(entity, "name"),
                NameAr = CRMOperations.GetValueByAttributeName<string>(entity, "ntw_companynamearabic"),
                Description = CRMOperations.GetValueByAttributeName<string>(entity, "description"),
                DescriptionAr = CRMOperations.GetValueByAttributeName<string>(entity, "ntw_descriptionar"),
                Logo = logoBytes != null ? Convert.ToBase64String(logoBytes) : null,
                Website = CRMOperations.GetValueByAttributeName<string>(entity, "websiteurl"),
                Country = CRMOperations.GetValueByAttributeName<string>(entity, "address1_country"),
                City = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "city.ntw_name")?.Value?.ToString(),
                CityAr = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "city.pwc_namear")?.Value?.ToString(),
                SectorId = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "sector.ntw_gicssectorid")?.Value?.ToString(),
                SectorName = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "sector.ntw_name")?.Value?.ToString(),
                SectorNameAr = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "sector.pwc_referenceidarabic")?.Value?.ToString(),
                DivisionId = null,
                DivisionName = null,
                DivisionNameAr = null,
                EstablishmentDate = CRMOperations.GetValueByAttributeName<DateTime?>(entity, "ntw_establishmentdate"),
                CreatedOn = CRMOperations.GetValueByAttributeName<DateTime?>(entity, "createdon"),
                Representative = new CompanyRepresentativeDto
                {
                    Name = $"{firstName} {lastName}".Trim(),
                    NameAr = $"{firstNameAr} {lastNameAr}".Trim(),
                    Position = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "position.ntw_name")?.Value?.ToString(),
                    PositionAr = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "position.ntw_namear")?.Value?.ToString(),
                    Email = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "representative.emailaddress1")?.Value?.ToString(),
                    Phone = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "representative.telephone1")?.Value?.ToString(),
                    Mobile = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "representative.mobilephone")?.Value?.ToString()
                }
            };
        }

        #endregion
    }
}
