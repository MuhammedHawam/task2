using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using PIF.EBP.Application.AccessManagement;
using PIF.EBP.Application.Contacts.Dtos;
using PIF.EBP.Application.EntitiesCache;
using PIF.EBP.Application.MetaData.DTOs;
using PIF.EBP.Application.PerformanceDashboard.DTOs;
using PIF.EBP.Application.PortalConfiguration;
using PIF.EBP.Application.Shared;
using PIF.EBP.Application.Shared.Helpers;
using PIF.EBP.Core.CRM;
using PIF.EBP.Core.Exceptions;
using PIF.EBP.Core.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static PIF.EBP.Application.Shared.Enums;

namespace PIF.EBP.Application.PerfomanceDashboard.Implementation
{
    public class PerformanceDashboardAppService : IPerformanceDashboardAppService
    {
        private readonly ICrmService _crmService;
        private readonly ISessionService _sessionService;
        private readonly IAccessManagementAppService _roleService;
        private readonly IPortalConfigAppService _portalConfigAppService;
        private readonly IAccessManagementCacheManager _accessManagementCacheManager;
        private readonly IEntitiesCacheAppService _entitiesCacheAppService;

        public PerformanceDashboardAppService(ICrmService crmService, ISessionService sessionService,
            IAccessManagementAppService roleService, IPortalConfigAppService portalConfigAppService, IAccessManagementCacheManager accessManagementCacheManager, IEntitiesCacheAppService entitiesCacheAppService)
        {
            _crmService = crmService;
            _sessionService = sessionService;
            _roleService = roleService;
            _portalConfigAppService = portalConfigAppService;
            _accessManagementCacheManager = accessManagementCacheManager;
            _entitiesCacheAppService = entitiesCacheAppService;
        }
        public async Task<Company> GetMyCompany()
        {
            return (await GetCompanyById("accountid", _sessionService.GetCompanyId())).FirstOrDefault();
        }

        public async Task<CompaniesDto> RetrieveCompanies(int pageNumber, int pageSize, string searchText, bool AllPIFCompanies = false)
        {
            var response = new CompaniesDto();
            var companiesDto = new List<Company>();
            int count = 0;

            if (AllPIFCompanies)
            {
                companiesDto = await GetCompaniesList("ntw_isitannounced", _sessionService.GetCompanyId(),
                    pageNumber, pageSize, searchText);

                count = await GetTotalCompaniesCount("ntw_isitannounced", _sessionService.GetCompanyId());
            }
            else
            {
                companiesDto = await GetCompaniesList(string.Empty, _sessionService.GetCompanyId(),
                    pageNumber, pageSize, searchText);

                count = await GetTotalCompaniesCount(string.Empty, _sessionService.GetCompanyId());
            }

            if (pageNumber == 1)
            {
                pageSize -= companiesDto.Count(x => x.IsPin == true);
            }

            response.ItemCount = count;
            response.companies = companiesDto;

            return response;
        }

        public async Task<CompanyOverviewDto> RetrieveCompanyOverview(Guid companyId)
        {
            var SessionCompanyId = _sessionService.GetCompanyId();
            if (companyId.ToString() != SessionCompanyId)
            {
                List<Guid> investors = await GetCompanyInvestors(SessionCompanyId);
                bool isCompanyInvestor = investors.Contains(companyId);
                if (!isCompanyInvestor)
                {
                    throw new UserFriendlyException("Unauthorized", System.Net.HttpStatusCode.Unauthorized);
                }
            }

            var query = new QueryExpression(EntityNames.Account)
            {
                ColumnSet = new ColumnSet("accountid", "name", "ntw_companynamearabic", "ntw_description", "ntw_descriptionar", "description", "ntw_overviewoncompanystrategy", "ntw_establishmentdate", "pwc_address", "websiteurl", "ntw_pcboardsecretary", "entityimage", "ntw_relationshipmanager"),
                Criteria = { Conditions = { new ConditionExpression("accountid", ConditionOperator.Equal, companyId) } },
                LinkEntities =
                {
                    new LinkEntity
                    {
                        LinkFromEntityName = EntityNames.Account,
                        LinkFromAttributeName = "ntw_pointofcontactid",
                        LinkToEntityName = EntityNames.Contact,
                        LinkToAttributeName = "contactid",
                        JoinOperator = JoinOperator.LeftOuter,
                        EntityAlias = "pointOfContactAlias",
                        Columns = new ColumnSet("entityimage", "firstname", "ntw_firstnamearabic", "lastname", "ntw_lastnamearabic", "emailaddress1", "contactid")
                    },
                    new LinkEntity
                    {
                        LinkFromEntityName = EntityNames.Account,
                        LinkFromAttributeName = "ntw_gicssectorid",
                        LinkToEntityName = EntityNames.GICSSector,
                        LinkToAttributeName = "ntw_gicssectorid",
                        JoinOperator = JoinOperator.LeftOuter,
                        EntityAlias = "GICSSectorAlias",
                        Columns = new ColumnSet("ntw_gicssectorid","pwc_referenceidarabic", "ntw_name", "pwc_flag")
                    },
                    new LinkEntity
                    {
                        LinkFromEntityName = EntityNames.Account,
                        LinkFromAttributeName = "ntw_relationshipmanager",
                        LinkToEntityName = EntityNames.SystemUser,
                        LinkToAttributeName = "systemuserid",
                        JoinOperator = JoinOperator.LeftOuter,
                        EntityAlias = "relationshipManagerAlias",
                        Columns = new ColumnSet("fullname", "systemuserid", "pwc_fullnamearabic")
                    }
                }
            };

            var entityCollection = _crmService.GetInstance().RetrieveMultiple(query);
            var companyOverviewDto = entityCollection.Entities.Select(FillCompanyOverviewRecord).FirstOrDefault();
            companyOverviewDto.ExecutiveManagement = await GetCompanyExecutiveManagement(companyId);

            return companyOverviewDto;
        }

        public async Task<CompanyKPIsMilestonesDto> RetrieveCompanyKPIsMilestones(CompanyKPIsMilestonesRequestDto companyKPIsMilestonesRequestDto)
        {
            CompanyKPIsMilestonesDto companyKPIsMilestonesDto = new CompanyKPIsMilestonesDto();

            if (companyKPIsMilestonesRequestDto.Scope == (int)CompanyKPIsMilestonesScope.All)
            {
                companyKPIsMilestonesRequestDto.PagingRequest.SortField = string.Empty;
            }

            if (companyKPIsMilestonesRequestDto.Scope == (int)CompanyKPIsMilestonesScope.All || companyKPIsMilestonesRequestDto.Scope == (int)CompanyKPIsMilestonesScope.KPIs)
            {
                companyKPIsMilestonesDto.CompanyKPIs = await GetCompanyKPIs(companyKPIsMilestonesRequestDto);
                companyKPIsMilestonesDto.KPIsCount = companyKPIsMilestonesDto.CompanyKPIs.Count();
                companyKPIsMilestonesDto.CompanyKPIs = companyKPIsMilestonesDto.CompanyKPIs.Skip((companyKPIsMilestonesRequestDto.PagingRequest.PageNo - 1) * companyKPIsMilestonesRequestDto.PagingRequest.PageSize).Take(companyKPIsMilestonesRequestDto.PagingRequest.PageSize).ToList();
            }

            if (companyKPIsMilestonesRequestDto.Scope == (int)CompanyKPIsMilestonesScope.All || companyKPIsMilestonesRequestDto.Scope == (int)CompanyKPIsMilestonesScope.Milestones)
            {
                companyKPIsMilestonesDto.CompanyMilestones = await GetCompanyMilestones(companyKPIsMilestonesRequestDto);
                companyKPIsMilestonesDto.MilestonesCount = companyKPIsMilestonesDto.CompanyMilestones.Count();
                companyKPIsMilestonesDto.CompanyMilestones = companyKPIsMilestonesDto.CompanyMilestones.Skip((companyKPIsMilestonesRequestDto.PagingRequest.PageNo - 1) * companyKPIsMilestonesRequestDto.PagingRequest.PageSize).Take(companyKPIsMilestonesRequestDto.PagingRequest.PageSize).ToList();
            }

            return companyKPIsMilestonesDto;
        }

        public async Task<CompanyGovernanceManagementDto> RetrieveCompanyGovernanceManagement(CompanyGovernanceManagementRequestDto companyGovernanceManagementRequestDto)
        {
            CompanyGovernanceManagementDto companyGovernanceManagementDto = new CompanyGovernanceManagementDto();

            if (companyGovernanceManagementRequestDto.Scope == (int)CompanyGovernanceManagementScope.All)
            {
                companyGovernanceManagementRequestDto.PagingRequest.SortField = string.Empty;
            }

            if (companyGovernanceManagementRequestDto.Scope == (int)CompanyGovernanceManagementScope.All || companyGovernanceManagementRequestDto.Scope == (int)CompanyGovernanceManagementScope.BorderMembers)
            {
                companyGovernanceManagementDto.BorderMembers = await GetCompanyBorderMembers(companyGovernanceManagementRequestDto);
                companyGovernanceManagementDto.BorderMembersCount = companyGovernanceManagementDto.BorderMembers.Count();
                companyGovernanceManagementDto.BorderMembers = companyGovernanceManagementDto.BorderMembers.Skip((companyGovernanceManagementRequestDto.PagingRequest.PageNo - 1) * companyGovernanceManagementRequestDto.PagingRequest.PageSize).Take(companyGovernanceManagementRequestDto.PagingRequest.PageSize).ToList();
            }
            if (companyGovernanceManagementRequestDto.Scope == (int)CompanyGovernanceManagementScope.All || companyGovernanceManagementRequestDto.Scope == (int)CompanyGovernanceManagementScope.EBPUsers)
            {
                companyGovernanceManagementDto.EBPUsers = await GetCompanyEBPUsers(companyGovernanceManagementRequestDto);
                companyGovernanceManagementDto.EBPUsersCount = companyGovernanceManagementDto.EBPUsers.Count();
                companyGovernanceManagementDto.EBPUsers = companyGovernanceManagementDto.EBPUsers.Skip((companyGovernanceManagementRequestDto.PagingRequest.PageNo - 1) * companyGovernanceManagementRequestDto.PagingRequest.PageSize).Take(companyGovernanceManagementRequestDto.PagingRequest.PageSize).ToList();
            }
            if (companyGovernanceManagementRequestDto.Scope == (int)CompanyGovernanceManagementScope.All || companyGovernanceManagementRequestDto.Scope == (int)CompanyGovernanceManagementScope.CommitteeMembers)
            {
                companyGovernanceManagementDto.CommitteeMembers = await GetCompanyCommitteeMembers(companyGovernanceManagementRequestDto);
                companyGovernanceManagementDto.CommitteeMembersCount = companyGovernanceManagementDto.CommitteeMembers.Count();
                companyGovernanceManagementDto.CommitteeMembers = companyGovernanceManagementDto.CommitteeMembers.Skip((companyGovernanceManagementRequestDto.PagingRequest.PageNo - 1) * companyGovernanceManagementRequestDto.PagingRequest.PageSize).Take(companyGovernanceManagementRequestDto.PagingRequest.PageSize).ToList();
            }

            return companyGovernanceManagementDto;
        }

        private async Task<List<Company>> GetCompanyById(string companyField, string companyId, string searchText = null)
        {
            var query = new QueryExpression(EntityNames.Account)
            {
                ColumnSet = new ColumnSet("accountid", "name", "ntw_companynamearabic", "ntw_headoffice", "websiteurl", "ntw_pcboardsecretary", "entityimage"),
                LinkEntities =
                {
                    new LinkEntity
                    {
                        LinkFromEntityName = EntityNames.Account,
                        LinkFromAttributeName = "ntw_pointofcontactid",
                        LinkToEntityName = EntityNames.Contact,
                        LinkToAttributeName = "contactid",
                        JoinOperator = JoinOperator.LeftOuter,
                        EntityAlias = "pointOfContactAlias",
                        Columns = new ColumnSet("entityimage", "firstname", "ntw_firstnamearabic", "lastname", "ntw_lastnamearabic", "emailaddress1", "contactid")
                    },
                    new LinkEntity
                    {
                        LinkFromEntityName = EntityNames.Account,
                        LinkFromAttributeName = "ntw_gicssectorid",
                        LinkToEntityName = EntityNames.GICSSector,
                        LinkToAttributeName = "ntw_gicssectorid",
                        JoinOperator = JoinOperator.LeftOuter,
                        EntityAlias = "GICSSectorAlias",
                        Columns = new ColumnSet("ntw_gicssectorid","pwc_referenceidarabic", "ntw_name", "pwc_flag")
                    },
                    new LinkEntity
                    {
                        LinkFromEntityName = EntityNames.Account,
                        LinkFromAttributeName = "accountid",
                        LinkToEntityName = EntityNames.PortalPinned,
                        LinkToAttributeName = "pwc_company",
                        JoinOperator = JoinOperator.LeftOuter,
                        Columns= new ColumnSet("pwc_company", "pwc_companyidid"),
                        EntityAlias = "Pin",
                        LinkCriteria =
                        {
                            Conditions =
                            {
                                new ConditionExpression("pwc_companyidid", ConditionOperator.Equal, _sessionService.GetCompanyId()),
                                new ConditionExpression("pwc_useridid", ConditionOperator.Equal, _sessionService.GetContactId()),
                            }
                        }
                    }
                }
            };

            if (companyField == "ntw_isitannounced")
            {
                query.Criteria.AddCondition(companyField, ConditionOperator.Equal, ((int)YesOrNo.Yes).ToString());
            }
            else if (companyField == "accountid")
            {
                query.Criteria.AddCondition(companyField, ConditionOperator.Equal, companyId);
            }
            else
            {
                List<Guid> investors = new List<Guid>();
                investors = await GetCompanyInvestors(companyId);

                if (investors.Count > 0)
                {
                    query.Criteria.AddCondition("accountid", ConditionOperator.In, investors.Cast<object>().ToArray());
                }
                else
                {
                    return new List<Company>();
                }
            }

            var entityCollection = _crmService.GetInstance().RetrieveMultiple(query);
            var companyDtoList = entityCollection.Entities.Select(FillCompanyRecord).ToList();

            if (!string.IsNullOrEmpty(searchText))
            {
                companyDtoList = companyDtoList.Where(x => (!string.IsNullOrEmpty(x.Name) && x.Name.ToLower().Contains(searchText.ToLower())) || (!string.IsNullOrEmpty(x.NameAr) && x.NameAr.ToLower().Contains(searchText.ToLower()))).ToList();
            }

            return companyDtoList;
        }

        public async Task<List<Company>> GetCompaniesList(string companyField, string companyId,
            int pageNumber, int pageSize, string searchText = null)
        {
            var query = new QueryExpression(EntityNames.Account)
            {
                ColumnSet = new ColumnSet("accountid", "name", "ntw_companynamearabic", "ntw_headoffice", "websiteurl", "ntw_pcboardsecretary", "entityimage"),
                LinkEntities =
                {
                    new LinkEntity
                    {
                        LinkFromEntityName = EntityNames.Account,
                        LinkFromAttributeName = "ntw_pointofcontactid",
                        LinkToEntityName = EntityNames.Contact,
                        LinkToAttributeName = "contactid",
                        JoinOperator = JoinOperator.LeftOuter,
                        EntityAlias = "pointOfContactAlias",
                        Columns = new ColumnSet("entityimage", "firstname", "ntw_firstnamearabic", "lastname", "ntw_lastnamearabic", "emailaddress1", "contactid")
                    },
                    new LinkEntity
                    {
                        LinkFromEntityName = EntityNames.Account,
                        LinkFromAttributeName = "ntw_gicssectorid",
                        LinkToEntityName = EntityNames.GICSSector,
                        LinkToAttributeName = "ntw_gicssectorid",
                        JoinOperator = JoinOperator.LeftOuter,
                        EntityAlias = "GICSSectorAlias",
                        Columns = new ColumnSet("ntw_gicssectorid","pwc_referenceidarabic", "ntw_name", "pwc_flag")
                    },
                    new LinkEntity
                    {
                        LinkFromEntityName = EntityNames.Account,
                        LinkFromAttributeName = "accountid",
                        LinkToEntityName = EntityNames.PortalPinned,
                        LinkToAttributeName = "pwc_company",
                        JoinOperator = JoinOperator.LeftOuter,
                        Columns= new ColumnSet("pwc_company", "pwc_companyidid"),
                        EntityAlias = "Pin",
                        LinkCriteria =
                        {
                            Conditions =
                            {
                                new ConditionExpression("pwc_companyidid", ConditionOperator.Equal, _sessionService.GetCompanyId()),
                                new ConditionExpression("pwc_useridid", ConditionOperator.Equal, _sessionService.GetContactId()),
                            }
                        }
                    }
                },
                PageInfo = new PagingInfo
                {
                    PageNumber = pageNumber,
                    Count = pageSize
                }
            };

            var order = new OrderExpression
            {
                AttributeName = "pwc_company",
                OrderType = (OrderType)OrderType.Descending
            };
            (query.LinkEntities.FirstOrDefault(le => le.EntityAlias == "Pin")).Orders.Add(order);

            if (companyField == "ntw_isitannounced")
            {
                query.Criteria.AddCondition(companyField, ConditionOperator.Equal, ((int)YesOrNo.Yes).ToString());
            }
            else if (companyField == "accountid")
            {
                query.Criteria.AddCondition(companyField, ConditionOperator.Equal, companyId);
            }
            else
            {
                List<Guid> investors = new List<Guid>();
                investors = await GetCompanyInvestors(companyId);

                if (investors.Count > 0)
                {
                    query.Criteria.AddCondition("accountid", ConditionOperator.In, investors.Cast<object>().ToArray());
                }
                else
                {
                    return new List<Company>();
                }
            }

            var entityCollection = _crmService.GetInstance().RetrieveMultiple(query);
            var companyDtoList = entityCollection.Entities.Select(FillCompanyRecord).ToList();

            if (!string.IsNullOrEmpty(searchText))
            {
                companyDtoList = companyDtoList.Where(x => (!string.IsNullOrEmpty(x.Name) && x.Name.ToLower().Contains(searchText.ToLower())) || (!string.IsNullOrEmpty(x.NameAr) && x.NameAr.ToLower().Contains(searchText.ToLower()))).ToList();
            }

            return companyDtoList;
        }

        private async Task<int> GetTotalCompaniesCount(string companyField, string companyId)
        {
            // Build the FetchXML query dynamically based on conditions
            var fetchXml = $@"
                        <fetch aggregate='true'>
                            <entity name='account'>
                                <attribute name='accountid' alias='companyCount' aggregate='count' />
                                <filter type='and'>
                                    {await GenerateFilterConditions(companyField, companyId)}
                                </filter>
                            </entity>
                        </fetch>";

            var fetchExpression = new FetchExpression(fetchXml);
            var result = _crmService.GetInstance().RetrieveMultiple(fetchExpression);

            var count = (int)((AliasedValue)result.Entities[0]["companyCount"]).Value;
            return count;
        }

        private async Task<string> GenerateFilterConditions(string companyField, string companyId)
        {
            if (companyField == "ntw_isitannounced")
            {
                return $"<condition attribute='{companyField}' operator='eq' value='{((int)YesOrNo.Yes)}' />";
            }
            else if (companyField == "accountid")
            {
                return $"<condition attribute='{companyField}' operator='eq' value='{companyId}' />";
            }
            else
            {
                var investors = await GetCompanyInvestors(companyId);

                if (!investors.Any())
                {
                    return "<condition attribute='accountid' operator='eq' value='00000000-0000-0000-0000-000000000000' />";
                }

                var investorConditions = string.Join("", investors.Select(id => $"<value>{id}</value>"));
                return $"<condition attribute='accountid' operator='in'>{investorConditions}</condition>";
            }
        }


        private async Task<List<Guid>> GetCompanyInvestors(string companyId)
        {
            var query = new QueryExpression(EntityNames.Investment)
            {
                ColumnSet = new ColumnSet(
                    "pwc_investmentid",
                    "pwc_childcompany",
                    "pwc_parentcompany"
                )
            };

            var linkEntity = new LinkEntity
            {
                LinkFromEntityName = EntityNames.Investment,
                LinkFromAttributeName = "pwc_parentcompany",
                LinkToEntityName = EntityNames.Account,
                LinkToAttributeName = "accountid",
                JoinOperator = JoinOperator.Inner,
                EntityAlias = "ad",
                LinkCriteria = new FilterExpression
                {
                    FilterOperator = LogicalOperator.And
                }
            };

            linkEntity.LinkCriteria.AddCondition(new ConditionExpression
            {
                AttributeName = "accountid",
                Operator = ConditionOperator.Equal,
                Values = { new Guid(companyId) }
            });

            query.LinkEntities.Add(linkEntity);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0 /*Active*/);

            var result = _crmService.GetInstance().RetrieveMultiple(query);

            List<Guid> childCompanyIds = new List<Guid>();

            foreach (var entity in result.Entities)
            {
                if (entity.Contains("pwc_childcompany") && entity["pwc_childcompany"] is EntityReference childCompanyRef)
                {
                    childCompanyIds.Add(childCompanyRef.Id);
                }
            }

            return childCompanyIds;
        }

        private async Task<List<BorderMembers>> GetCompanyBorderMembers(CompanyGovernanceManagementRequestDto companyGovernanceManagementRequestDto)
        {
            var query = new QueryExpression(EntityNames.Membership)
            {
                ColumnSet = new ColumnSet(
                    "ntw_companyname",
                    "ntw_membershipid",
                    "ntw_contactname",
                    "ntw_memberposition",
                    "ntw_membershipaffiliationset",
                     "ntw_membershiptypeid"
                ),
                Criteria = new FilterExpression
                {
                    FilterOperator = LogicalOperator.And,
                    Conditions =
                    {
                        new ConditionExpression("ntw_companyname", ConditionOperator.Equal, companyGovernanceManagementRequestDto.CompanyId)
                    }
                },
                LinkEntities =
                {
                    new LinkEntity
                    {
                        LinkFromEntityName = EntityNames.Membership,
                        LinkFromAttributeName = "ntw_contactname",
                        LinkToEntityName = EntityNames.Contact,
                        LinkToAttributeName = "contactid",
                        JoinOperator = JoinOperator.LeftOuter,
                        EntityAlias = "contactAlias",
                        Columns = new ColumnSet("emailaddress1","entityimage", "firstname", "lastname", "ntw_firstnamearabic", "ntw_lastnamearabic","ntw_titleset")
                    }
                }
            };

            var membershipTypeIds = GetMembershipTypes(new List<int> { (int)Enums.MembershipType.PCBoardMember, (int)Enums.MembershipType.PCBoardChairman, (int)Enums.MembershipType.PCBoardViceChairman });
            if (membershipTypeIds.Any())
            {
                query.Criteria.AddCondition("ntw_membershiptypeid", ConditionOperator.In, membershipTypeIds.Cast<object>().ToArray());
            }

            if (!string.IsNullOrEmpty(companyGovernanceManagementRequestDto?.PagingRequest?.SortField))
            {
                var mappedColmnsDics = new Dictionary<string, string>
                {
                    { "contactAlias.firstname", "Name" },
                    { "contactAlias.ntw_firstnamearabic", "NameAr" },
                    { "pwc_unitofmeasurement", "UnitOfMeasurement" },
                    { "ntw_membershipaffiliationset", "Affiliation" },
                    { "ntw_memberposition", "Position"}
                };

                if (companyGovernanceManagementRequestDto.PagingRequest.SortField.ToLower() == mappedColmnsDics.FirstOrDefault(x => x.Key == "contactAlias.firstname").Value.ToString().ToLower())
                {
                    companyGovernanceManagementRequestDto.PagingRequest.SortField = "firstname";
                    CRMOperations.AddOrderToLinkEntity(query.LinkEntities.FirstOrDefault(le => le.EntityAlias == "contactAlias"), companyGovernanceManagementRequestDto.PagingRequest);
                }
                else if (companyGovernanceManagementRequestDto.PagingRequest.SortField.ToLower() == mappedColmnsDics.FirstOrDefault(x => x.Key == "contactAlias.ntw_firstnamearabic").Value.ToString().ToLower())
                {
                    companyGovernanceManagementRequestDto.PagingRequest.SortField = "ntw_firstnamearabic";
                    CRMOperations.AddOrderToLinkEntity(query.LinkEntities.FirstOrDefault(le => le.EntityAlias == "contactAlias"), companyGovernanceManagementRequestDto.PagingRequest);
                }
                else
                {
                    CRMOperations.GetQueryExpression(query, companyGovernanceManagementRequestDto.PagingRequest, mappedColmnsDics);
                }
            }

            var entityCollection = _crmService.GetInstance().RetrieveMultiple(query);

            var companyBorderMembersDto = new List<BorderMembers>();
            foreach (var entity in entityCollection.Entities)
            {
                var member = await FillCompanyBorderMembers(entity);
                companyBorderMembersDto.Add(member);
            }

            if (string.IsNullOrEmpty(companyGovernanceManagementRequestDto?.PagingRequest?.SortField))
            {
                // Define the custom sort order
                var positionSortOrder = new Dictionary<string, int>
            {
                { "chairman", 1 },
                { "vice chairman", 2 },
                { "secretary", 3 },
                { "member", 4 },
                { "Invited guest", 5 }
            };
                // Apply sorting based on the custom order
                companyBorderMembersDto = companyBorderMembersDto
                    .OrderBy(m => positionSortOrder.ContainsKey(m.Position.Name.ToLower()) ? positionSortOrder[m.Position.Name.ToLower()] : int.MaxValue)
                    .ToList();
            }
            return companyBorderMembersDto;
        }

        private async Task<List<EBPUsers>> GetCompanyEBPUsers(CompanyGovernanceManagementRequestDto companyGovernanceManagementRequestDto)
        {
            var query = new QueryExpression(EntityNames.ContactAssociation)
            {
                ColumnSet = new ColumnSet(
                    "hexa_portalroleid",
                    "hexa_email",
                    "hexa_contactid",
                    "hexa_companyid",
                    "hexa_contactroleassociationid",
                    "pwc_positionid"
                ),
                Criteria = new FilterExpression
                {
                    FilterOperator = LogicalOperator.And,
                    Conditions =
                    {
                        new ConditionExpression("hexa_companyid", ConditionOperator.Equal, companyGovernanceManagementRequestDto.CompanyId)
                    }
                },
                LinkEntities =
                {
                    new LinkEntity
                    {
                        LinkFromEntityName = EntityNames.ContactAssociation,
                        LinkFromAttributeName = "hexa_contactid",
                        LinkToEntityName = EntityNames.Contact,
                        LinkToAttributeName = "contactid",
                        JoinOperator = JoinOperator.LeftOuter,
                        EntityAlias = "contactAlias",
                        Columns = new ColumnSet("emailaddress1","entityimage", "firstname", "lastname", "ntw_firstnamearabic", "ntw_lastnamearabic")
                    }
                }
            };

            if (!string.IsNullOrEmpty(companyGovernanceManagementRequestDto?.PagingRequest?.SortField))
            {
                var mappedColmnsDics = new Dictionary<string, string>
                {
                    { "contactAlias.firstname", "Name" },
                    { "contactAlias.ntw_firstnamearabic", "NameAr" },
                    { "pwc_positionid", "Position" },
                    { "hexa_portalroleid", "Role" },
                    { "hexa_email", "ContactDetailsEmail"}
                };

                if (companyGovernanceManagementRequestDto.PagingRequest.SortField.ToLower() == mappedColmnsDics.FirstOrDefault(x => x.Key == "contactAlias.firstname").Value.ToString().ToLower())
                {
                    companyGovernanceManagementRequestDto.PagingRequest.SortField = "firstname";
                    CRMOperations.AddOrderToLinkEntity(query.LinkEntities.FirstOrDefault(le => le.EntityAlias == "contactAlias"), companyGovernanceManagementRequestDto.PagingRequest);
                }
                else if (companyGovernanceManagementRequestDto.PagingRequest.SortField.ToLower() == mappedColmnsDics.FirstOrDefault(x => x.Key == "contactAlias.ntw_firstnamearabic").Value.ToString().ToLower())
                {
                    companyGovernanceManagementRequestDto.PagingRequest.SortField = "ntw_firstnamearabic";
                    CRMOperations.AddOrderToLinkEntity(query.LinkEntities.FirstOrDefault(le => le.EntityAlias == "contactAlias"), companyGovernanceManagementRequestDto.PagingRequest);
                }
                else
                {
                    CRMOperations.GetQueryExpression(query, companyGovernanceManagementRequestDto.PagingRequest, mappedColmnsDics);
                }
            }

            var entityCollection = _crmService.GetInstance().RetrieveMultiple(query);

            var companyEBPUsersDto = new List<EBPUsers>();
            foreach (var entity in entityCollection.Entities)
            {
                var user = await FillCompanyEBPUsers(entity);
                companyEBPUsersDto.Add(user);
            }

            return companyEBPUsersDto;
        }

        private async Task<List<CommitteeMember>> GetCompanyCommitteeMembers(CompanyGovernanceManagementRequestDto companyGovernanceManagementRequestDto)
        {
            var query = new QueryExpression(EntityNames.Membership)
            {
                ColumnSet = new ColumnSet(
                    "ntw_companyname",
                    "ntw_membershipid",
                    "ntw_pccommitteeid",
                    "ntw_memberposition",
                    "ntw_contactname",
                    "ntw_membershiptypeid"
                ),
                Criteria = new FilterExpression
                {
                    FilterOperator = LogicalOperator.And,
                    Conditions =
                    {
                        new ConditionExpression("ntw_companyname", ConditionOperator.Equal, companyGovernanceManagementRequestDto.CompanyId)
                    }
                },
                LinkEntities =
                {
                    new LinkEntity
                    {
                        LinkFromEntityName = EntityNames.Membership,
                        LinkFromAttributeName = "ntw_pccommitteeid",
                        LinkToEntityName = EntityNames.PcCommittee,
                        LinkToAttributeName = "ntw_pccommitteeid",
                        JoinOperator = JoinOperator.LeftOuter,
                        EntityAlias = "committeeAlias",
                        Columns = new ColumnSet("ntw_name", "ntw_boardcommitteenamearabic")
                    },
                    new LinkEntity
                    {
                        LinkFromEntityName = EntityNames.Membership,
                        LinkFromAttributeName = "ntw_contactname",
                        LinkToEntityName = EntityNames.Contact,
                        LinkToAttributeName = "contactid",
                        JoinOperator = JoinOperator.Inner,
                        EntityAlias = "ContactAlias",
                        Columns = new ColumnSet( "firstname", "lastname", "ntw_firstnamearabic", "ntw_lastnamearabic","ntw_titleset")
                    }
                }

            };
            var membershipTypeIds = GetMembershipTypes(new List<int> { (int)Enums.MembershipType.PCCommitteeMember, (int)Enums.MembershipType.PCCommitteeChairman, (int)Enums.MembershipType.PCCommitteeViceChairman });
            if (membershipTypeIds.Any())
            {
                query.Criteria.AddCondition("ntw_membershiptypeid", ConditionOperator.In, membershipTypeIds.Cast<object>().ToArray());
            }

            if (!string.IsNullOrEmpty(companyGovernanceManagementRequestDto?.PagingRequest?.SortField))
            {
                var mappedColmnsDics = new Dictionary<string, string>
                {
                    { "ContactAlias.firstname", "Name" },
                    { "ContactAlias.ntw_firstnamearabic", "NameAr" },
                    { "committeeAlias.ntw_name", "CommitteeName" },
                    { "committeeAlias.ntw_boardcommitteenamearabic", "CommitteeNameAr" },
                    { "ntw_memberposition", "Position" }
                };

                if (companyGovernanceManagementRequestDto.PagingRequest.SortField.ToLower() == mappedColmnsDics.FirstOrDefault(x => x.Key == "committeeAlias.ntw_name").Value.ToString().ToLower())
                {
                    companyGovernanceManagementRequestDto.PagingRequest.SortField = "ntw_name";
                    CRMOperations.AddOrderToLinkEntity(query.LinkEntities.FirstOrDefault(le => le.EntityAlias == "committeeAlias"), companyGovernanceManagementRequestDto.PagingRequest);
                }
                else if (companyGovernanceManagementRequestDto.PagingRequest.SortField.ToLower() == mappedColmnsDics.FirstOrDefault(x => x.Key == "committeeAlias.ntw_boardcommitteenamearabic").Value.ToString().ToLower())
                {
                    companyGovernanceManagementRequestDto.PagingRequest.SortField = "ntw_boardcommitteenamearabic";
                    CRMOperations.AddOrderToLinkEntity(query.LinkEntities.FirstOrDefault(le => le.EntityAlias == "committeeAlias"), companyGovernanceManagementRequestDto.PagingRequest);
                }
                else if (companyGovernanceManagementRequestDto.PagingRequest.SortField.ToLower() == mappedColmnsDics.FirstOrDefault(x => x.Key == "ContactAlias.firstname").Value.ToString().ToLower())
                {
                    companyGovernanceManagementRequestDto.PagingRequest.SortField = "firstname";
                    CRMOperations.AddOrderToLinkEntity(query.LinkEntities.FirstOrDefault(le => le.EntityAlias == "ContactAlias"), companyGovernanceManagementRequestDto.PagingRequest);
                }
                else if (companyGovernanceManagementRequestDto.PagingRequest.SortField.ToLower() == mappedColmnsDics.FirstOrDefault(x => x.Key == "ContactAlias.ntw_firstnamearabic").Value.ToString().ToLower())
                {
                    companyGovernanceManagementRequestDto.PagingRequest.SortField = "ntw_firstnamearabic";
                    CRMOperations.AddOrderToLinkEntity(query.LinkEntities.FirstOrDefault(le => le.EntityAlias == "ContactAlias"), companyGovernanceManagementRequestDto.PagingRequest);
                }
                else
                {
                    CRMOperations.GetQueryExpression(query, companyGovernanceManagementRequestDto.PagingRequest, mappedColmnsDics);
                }
            }

            var entityCollection = _crmService.GetInstance().RetrieveMultiple(query);
            var companyCommitteeMembersDto = entityCollection.Entities.Select(FillCompanyCommitteeMembers).ToList();

            if (string.IsNullOrEmpty(companyGovernanceManagementRequestDto?.PagingRequest?.SortField))
            {
                // Define the custom sort order
                var positionSortOrder = new Dictionary<string, int>
            {
                { "chairman", 1 },
                { "vice chairman", 2 },
                { "secretary", 3 },
                { "member", 4 },
                { "Invited guest", 5 }
            };
                // Apply sorting based on the custom order
                companyCommitteeMembersDto = companyCommitteeMembersDto
                    .OrderBy(m => positionSortOrder.ContainsKey(m.Position.Name.ToLower()) ? positionSortOrder[m.Position.Name.ToLower()] : int.MaxValue)
                    .ToList();
            }
            return companyCommitteeMembersDto;
        }

        private async Task<List<CompanyKPIsDto>> GetCompanyKPIs(CompanyKPIsMilestonesRequestDto companyKPIsMilestonesRequestDto)
        {
            var query = new QueryExpression(EntityNames.KeyPerformanceIndicatorKPI)
            {
                ColumnSet = new ColumnSet("pwc_keyperformanceindicatorkpiid", "pwc_name", "pwc_kpidescription", "pwc_unitofmeasurement", "pwc_metrictypetypecode", "pwc_metricstatustypecode", "pwc_weight", "pwc_companyid"),
                Criteria = new FilterExpression
                {
                    FilterOperator = LogicalOperator.And,
                    Conditions =
                    {
                        new ConditionExpression("pwc_companyid", ConditionOperator.Equal, companyKPIsMilestonesRequestDto.CompanyId)
                    }
                }
            };
            if (!string.IsNullOrEmpty(companyKPIsMilestonesRequestDto?.PagingRequest?.SortField))
            {
                var mappedColmnsDics = new Dictionary<string, string>
                {
                    { "pwc_kpidescription", "Descripcion" },
                    { "pwc_name", "MetricName" },
                    { "pwc_unitofmeasurement", "UnitOfMeasurement" },
                    { "pwc_metrictypetypecode", "MetricType" },
                    { "pwc_metricstatustypecode", "MetricStatus"},
                    { "pwc_weight", "Weight"}
                };

                CRMOperations.GetQueryExpression(query, companyKPIsMilestonesRequestDto.PagingRequest, mappedColmnsDics);
            }

            var entityCollection = _crmService.GetInstance().RetrieveMultiple(query);
            var companyKPIsDto = entityCollection.Entities.Select(FillCompanyKPIs).ToList();

            return companyKPIsDto;
        }

        private async Task<List<CompanyMilestonesDto>> GetCompanyMilestones(CompanyKPIsMilestonesRequestDto companyKPIsMilestonesRequestDto)
        {
            var query = new QueryExpression(EntityNames.Milestone)
            {
                ColumnSet = new ColumnSet("pwc_milestoneid", "pwc_name", "pwc_dateofcompletion", "pwc_statustypecode", "pwc_startdate", "pwc_baselineenddate"),
                Criteria = new FilterExpression
                {
                    FilterOperator = LogicalOperator.And,
                    Conditions =
                    {
                        new ConditionExpression("pwc_companyid", ConditionOperator.Equal, companyKPIsMilestonesRequestDto.CompanyId)
                    }
                }
            };

            if (!string.IsNullOrEmpty(companyKPIsMilestonesRequestDto?.PagingRequest?.SortField))
            {
                var mappedColmnsDics = new Dictionary<string, string>
                {
                    { "pwc_name", "Name" },
                    { "pwc_dateofcompletion", "CompletionDate" },
                    { "pwc_statustypecode", "Status" },
                    { "pwc_startdate", "StartDate" },
                    { "pwc_baselineenddate", "BaseLineEndDate"}
                };

                CRMOperations.GetQueryExpression(query, companyKPIsMilestonesRequestDto.PagingRequest, mappedColmnsDics);
            }

            var entityCollection = _crmService.GetInstance().RetrieveMultiple(query);
            var companyMilestonesDto = entityCollection.Entities.Select(FillCompanyMilestones).ToList();

            return companyMilestonesDto;
        }

        private async Task<List<ExecutiveManagementDto>> GetCompanyExecutiveManagement(Guid companyId)
        {
            var query = new QueryExpression(EntityNames.Experience)
            {
                ColumnSet = new ColumnSet("createdon", "ntw_positionid", "ntw_contactname", "ntw_companyname", "ntw_experienceid"),
                Criteria = new FilterExpression
                {
                    FilterOperator = LogicalOperator.And,
                    Conditions =
                    {
                        new ConditionExpression("ntw_companyname", ConditionOperator.Equal, companyId),
                        new ConditionExpression("ntw_ishesheexecutiveset", ConditionOperator.Equal, (int)ExecutiveManagement.Yes)
                    }
                },
                LinkEntities =
                {
                    new LinkEntity
                    {
                        LinkFromEntityName = EntityNames.Experience,
                        LinkFromAttributeName = "ntw_contactname",
                        LinkToEntityName = EntityNames.Contact,
                        LinkToAttributeName = "contactid",
                        JoinOperator = JoinOperator.Inner,
                        EntityAlias = "contactAlias",
                        Columns = new ColumnSet("emailaddress1","entityimage", "firstname", "lastname", "ntw_firstnamearabic", "ntw_lastnamearabic"),
                        LinkCriteria = new FilterExpression()
                    }
                }
            };

            var entityCollection = _crmService.GetInstance().RetrieveMultiple(query);
            var executiveManagementDtoList = entityCollection.Entities.Select(FillCompanyExecutiveManagementRecord).ToList();

            return executiveManagementDtoList;
        }

        private async Task<BorderMembers> FillCompanyBorderMembers(Entity entity)
        {
            var department = await GetContactDepartment(
                entity.GetAttributeValue<EntityReference>("ntw_contactname")?.Id,
                entity.GetAttributeValue<EntityReference>("ntw_companyname")?.Id
            );

            var contactImage = entity.GetValueByAttributeName<AliasedValue>("contactAlias.entityimage")?.Value ?? null;
            return new BorderMembers
            {
                ContactId = entity.GetValueByAttributeName<EntityReference>("ntw_contactname")?.Id??Guid.Empty,
                Title = _entitiesCacheAppService.RetrieveOptionSetCacheByKeyWithValue(OptionSetKey.ContactTitleType, (OptionSetValue)CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "contactAlias.ntw_titleset")?.Value),
                Name = (((entity.GetValueByAttributeName<AliasedValue>("contactAlias.firstname")?.Value.ToString() ?? string.Empty) + " " + (entity.GetValueByAttributeName<AliasedValue>("contactAlias.lastname")?.Value.ToString() ?? string.Empty))).Trim(),
                NameAr = ((entity.GetValueByAttributeName<AliasedValue>("contactAlias.ntw_firstnamearabic")?.Value.ToString() ?? string.Empty) + " " + (entity.GetValueByAttributeName<AliasedValue>("contactAlias.ntw_lastnamearabic")?.Value.ToString() ?? string.Empty)).Trim(),
                EntityImage = contactImage != null ? (byte[])contactImage : null,
                Affiliation =  entity.GetValueByAttributeName<EntityOptionSetDto>("ntw_membershipaffiliationset"),
                Position =  entity.GetValueByAttributeName<EntityOptionSetDto>("ntw_memberposition"),
                Department = department
            };
        }

        private async Task<EBPUsers> FillCompanyEBPUsers(Entity entity)
        {
            var department = await GetContactDepartment(
                entity.GetAttributeValue<EntityReference>("hexa_contactid")?.Id,
                entity.GetAttributeValue<EntityReference>("hexa_companyid")?.Id
            );

            var contactImage = entity.GetValueByAttributeName<AliasedValue>("contactAlias.entityimage")?.Value ?? null;

            return new EBPUsers
            {
                Name = ((entity.GetValueByAttributeName<AliasedValue>("contactAlias.firstname")?.Value.ToString() ?? string.Empty) + " " + (entity.GetValueByAttributeName<AliasedValue>("contactAlias.lastname")?.Value.ToString() ?? string.Empty)).Trim(),
                NameAr = ((entity.GetValueByAttributeName<AliasedValue>("contactAlias.ntw_firstnamearabic")?.Value.ToString() ?? string.Empty) + " " + (entity.GetValueByAttributeName<AliasedValue>("contactAlias.ntw_lastnamearabic")?.Value.ToString() ?? string.Empty)).Trim(),
                EntityImage = contactImage != null ? (byte[])contactImage : null,
                ContactDetailsEmail = entity.Contains("hexa_email") ? entity.GetValueByAttributeName<string>("hexa_email") : string.Empty,
                Position = entity.Contains("pwc_positionid") ? entity.GetValueByAttributeName<EntityReferenceDto>("pwc_positionid") : null,
                Role = entity.Contains("hexa_portalroleid") ? entity.GetValueByAttributeName<EntityReferenceDto>("hexa_portalroleid") : null,
                Department = department
            };
        }

        private CommitteeMember FillCompanyCommitteeMembers(Entity entity)
        {
            var name = $"{entity.GetValueByAttributeName<AliasedValue>("ContactAlias.firstname")?.Value.ToString() ?? string.Empty} {entity.GetAttributeValue<AliasedValue>("ContactAlias.lastname")?.Value.ToString() ?? string.Empty}".Trim();
            var nameAr = $"{entity.GetValueByAttributeName<AliasedValue>("ContactAlias.ntw_firstnamearabic")?.Value.ToString() ?? string.Empty} {entity.GetAttributeValue<AliasedValue>("ContactAlias.ntw_lastnamearabic")?.Value.ToString() ?? string.Empty}".Trim();
            return new CommitteeMember
            {
                ContactId = entity.GetValueByAttributeName<EntityReference>("ntw_contactname")?.Id??Guid.Empty,
                Title  = _entitiesCacheAppService.RetrieveOptionSetCacheByKeyWithValue(OptionSetKey.ContactTitleType, (OptionSetValue)CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "ContactAlias.ntw_titleset")?.Value),
                Name = name,
                NameAr = nameAr,
                CommitteeName = entity.GetValueByAttributeName<AliasedValue>("committeeAlias.ntw_name")?.Value.ToString() ?? string.Empty,
                CommitteeNameAr = entity.GetValueByAttributeName<AliasedValue>("committeeAlias.ntw_boardcommitteenamearabic")?.Value.ToString() ?? string.Empty,
                Position = entity.GetValueByAttributeName<EntityOptionSetDto>("ntw_memberposition")
            };
        }

        private CompanyKPIsDto FillCompanyKPIs(Entity entity)
        {
            return new CompanyKPIsDto
            {
                Descripcion = entity.Contains("pwc_kpidescription") ? entity.GetValueByAttributeName<string>("pwc_kpidescription") : string.Empty,
                DescripcionAr = entity.Contains("pwc_kpidescription") ? entity.GetValueByAttributeName<string>("pwc_kpidescription") : string.Empty,
                MetricName = entity.Contains("pwc_name") ? entity.GetValueByAttributeName<string>("pwc_name") : string.Empty,
                MetricNameAr = entity.Contains("pwc_name") ? entity.GetValueByAttributeName<string>("pwc_name") : string.Empty,
                UnitOfMeasurement = entity.Contains("pwc_unitofmeasurement") ? entity.GetValueByAttributeName<string>("pwc_unitofmeasurement") : string.Empty,
                UnitOfMeasurementAr = entity.Contains("pwc_unitofmeasurement") ? entity.GetValueByAttributeName<string>("pwc_unitofmeasurement") : string.Empty,
                Weight = entity.Contains("pwc_weight") ? entity.GetValueByAttributeName<decimal>("pwc_weight").ToString("F2") : string.Empty,
                MetricStatus = entity.Contains("pwc_metricstatustypecode") ? entity.GetValueByAttributeName<EntityOptionSetDto>("pwc_metricstatustypecode") : null,
                MetricType = entity.Contains("pwc_metrictypetypecode") ? entity.GetValueByAttributeName<EntityOptionSetDto>("pwc_metrictypetypecode") : null
            };
        }

        private CompanyMilestonesDto FillCompanyMilestones(Entity entity)
        {
            return new CompanyMilestonesDto
            {
                Name = entity.Contains("pwc_name") ? entity.GetValueByAttributeName<string>("pwc_name") : string.Empty,
                NameAr = entity.Contains("pwc_name") ? entity.GetValueByAttributeName<string>("pwc_name") : string.Empty,
                Status = entity.Contains("pwc_statustypecode") ? entity.GetValueByAttributeName<EntityOptionSetDto>("pwc_statustypecode") : null,
                BaseLineEndDate = entity.Contains("pwc_baselineenddate") ? entity.GetValueByAttributeName<DateTime>("pwc_baselineenddate") : (DateTime?)null,
                CompletionDate = entity.Contains("pwc_dateofcompletion") ? entity.GetValueByAttributeName<DateTime>("pwc_dateofcompletion") : (DateTime?)null,
                StartDate = entity.Contains("pwc_startdate") ? entity.GetValueByAttributeName<DateTime>("pwc_startdate") : (DateTime?)null,
            };
        }

        private ExecutiveManagementDto FillCompanyExecutiveManagementRecord(Entity entity)
        {
            var contactImage = entity.GetValueByAttributeName<AliasedValue>("contactAlias.entityimage")?.Value ?? null;

            return new ExecutiveManagementDto
            {
                Position = CRMOperations.GetValueByAttributeName<EntityReferenceDto>(entity, "ntw_positionid", "ntw_namear"),
                entityImage = contactImage != null ? (byte[])contactImage : null,
                Name = ((entity.GetValueByAttributeName<AliasedValue>("contactAlias.firstname")?.Value.ToString() ?? string.Empty) + " " + (entity.GetValueByAttributeName<AliasedValue>("contactAlias.lastname")?.Value.ToString() ?? string.Empty)).Trim(),
                NameAr = ((entity.GetValueByAttributeName<AliasedValue>("contactAlias.ntw_firstnamearabic")?.Value.ToString() ?? string.Empty) + " " + (entity.GetValueByAttributeName<AliasedValue>("contactAlias.ntw_lastnamearabic")?.Value.ToString() ?? string.Empty)).Trim()
            };
        }

        private CompanyOverviewDto FillCompanyOverviewRecord(Entity entity)
        {
            var pointOfContactImage = entity.GetValueByAttributeName<AliasedValue>("pointOfContactAlias.entityimage")?.Value ?? null;

            var serviceProvider = new ServiceProvider
            {
                Id = entity.Contains("GICSSectorAlias.ntw_gicssectorid") ? new Guid(entity.GetValueByAttributeName<AliasedValue>("GICSSectorAlias.ntw_gicssectorid")?.Value.ToString()) : Guid.Empty,
                Name = entity.GetValueByAttributeName<AliasedValue>("GICSSectorAlias.ntw_name")?.Value.ToString() ?? string.Empty,
                NameAr = entity.GetValueByAttributeName<AliasedValue>("GICSSectorAlias.pwc_referenceidarabic")?.Value.ToString() ?? string.Empty,
                Flag = entity.GetValueByAttributeName<AliasedValue>("GICSSectorAlias.pwc_flag")?.Value.ToString() ?? string.Empty,
            };

            var contactName = entity.GetValueByAttributeName<AliasedValue>("pointOfContactAlias.firstname")?.Value.ToString() + " " + entity.GetValueByAttributeName<AliasedValue>("pointOfContactAlias.lastname")?.Value.ToString();
            var contactNameAr = entity.GetValueByAttributeName<AliasedValue>("pointOfContactAlias.ntw_firstnamearabic")?.Value.ToString() + " " + entity.GetValueByAttributeName<AliasedValue>("pointOfContactAlias.ntw_lastnamearabic")?.Value.ToString();


            var relationshipManager = new RelationshipManagerDto
            {
                FullName = entity.GetValueByAttributeName<AliasedValue>("relationshipManagerAlias.fullname")?.Value.ToString(),
                SystemUserId = entity.GetValueByAttributeName<AliasedValue>("relationshipManagerAlias.systemuserid")?.Value.ToString(),
                FullNameAr = entity.GetValueByAttributeName<AliasedValue>("relationshipManagerAlias.pwc_fullnamearabic")?.Value.ToString(),
            };

            return new CompanyOverviewDto
            {
                CompanyName = entity.Contains("name") ? entity.GetValueByAttributeName<string>("name") : string.Empty,
                CompanyNameAr = entity.Contains("ntw_companynamearabic") ? entity.GetValueByAttributeName<string>("ntw_companynamearabic") : string.Empty,
                CompanyImage = entity.Contains("entityimage") ? entity.GetValueByAttributeName<byte[]>("entityimage") : null,
                ServiceProvider = serviceProvider,
                Address = GetCustomerAddressByCompanyId(entity.Id),
                EstablishmentDate = entity.Contains("ntw_establishmentdate") ? entity.GetAttributeValue<DateTime>("ntw_establishmentdate") : (DateTime?)null,
                Overview = entity.Contains("description") ? entity.GetAttributeValue<string>("description") : string.Empty,
                OverviewAr = entity.Contains("ntw_descriptionar") ? entity.GetAttributeValue<string>("ntw_descriptionar") : string.Empty,
                Website = entity.Contains("websiteurl") ? entity.GetAttributeValue<string>("websiteurl") : string.Empty,
                PointOfContactImage = pointOfContactImage != null ? (byte[])pointOfContactImage : null,
                PointOfContactName = contactName.Trim() ?? string.Empty,
                PointOfContactNameAr = contactNameAr.Trim() ?? string.Empty,
                PointOfContactEmail = entity.GetValueByAttributeName<AliasedValue>("pointOfContactAlias.emailaddress1")?.Value.ToString(),
                PointOfContactId = entity.GetValueByAttributeName<AliasedValue>("pointOfContactAlias.contactid")?.Value.ToString(),
                RelationshipManager = relationshipManager
            };
        }

        private Company FillCompanyRecord(Entity entity)
        {
            var pointOfContactImage = entity.GetValueByAttributeName<AliasedValue>("pointOfContactAlias.entityimage")?.Value ?? null;

            var serviceProvider = new ServiceProvider
            {
                Id = entity.Contains("GICSSectorAlias.ntw_gicssectorid") ? new Guid(entity.GetValueByAttributeName<AliasedValue>("GICSSectorAlias.ntw_gicssectorid")?.Value.ToString()) : Guid.Empty,
                Name = entity.GetValueByAttributeName<AliasedValue>("GICSSectorAlias.ntw_name")?.Value.ToString() ?? string.Empty,
                NameAr = entity.GetValueByAttributeName<AliasedValue>("GICSSectorAlias.pwc_referenceidarabic")?.Value.ToString() ?? string.Empty,
                Flag = entity.GetValueByAttributeName<AliasedValue>("GICSSectorAlias.pwc_flag")?.Value.ToString() ?? string.Empty,
            };

            var contactName = entity.GetValueByAttributeName<AliasedValue>("pointOfContactAlias.firstname")?.Value.ToString() + " " + entity.GetValueByAttributeName<AliasedValue>("pointOfContactAlias.lastname")?.Value.ToString();
            var contactNameAr = entity.GetValueByAttributeName<AliasedValue>("pointOfContactAlias.ntw_firstnamearabic")?.Value.ToString() + " " + entity.GetValueByAttributeName<AliasedValue>("pointOfContactAlias.ntw_lastnamearabic")?.Value.ToString();

            var isPin = entity.GetValueByAttributeName<AliasedValue>("Pin.pwc_companyidid")?.Value ?? null;

            return new Company
            {
                Id = entity.Id.ToString(),
                Name = entity.Contains("name") ? entity.GetValueByAttributeName<string>("name") : string.Empty,
                NameAr = entity.Contains("ntw_companynamearabic") ? entity.GetValueByAttributeName<string>("ntw_companynamearabic") : string.Empty,
                EntityImage = entity.Contains("entityimage") ? entity.GetValueByAttributeName<byte[]>("entityimage") : null,
                HeadQuarter = entity.Contains("ntw_headoffice") ? entity.GetValueByAttributeName<string>("ntw_headoffice") : string.Empty,
                WebSite = entity.Contains("websiteurl") ? entity.GetValueByAttributeName<string>("websiteurl") : string.Empty,
                PointOfContactEmail = entity.GetValueByAttributeName<AliasedValue>("pointOfContactAlias.emailaddress1")?.Value.ToString() ?? string.Empty,
                PointOfContactImage = pointOfContactImage != null ? (byte[])pointOfContactImage : null,
                PointOfContactName = contactName.Trim() ?? string.Empty,
                PointOfContactNameAr = contactNameAr.Trim() ?? string.Empty,
                PointOfContactId = entity.GetValueByAttributeName<AliasedValue>("pointOfContactAlias.contactid")?.Value.ToString() ?? string.Empty,
                ServiceProvider = serviceProvider,
                IsPin = isPin != null ? true : false
            };
        }

        private async Task<EntityReferenceDto> GetContactDepartment(Guid? contactId, Guid? companyId)
        {
            if (contactId.HasValue && contactId != Guid.Empty && companyId.HasValue && companyId != Guid.Empty)
            {
                var roles = await _roleService.GetContactRolesByContactId(contactId.ToString());

                if (roles?.Count > 0)
                {
                    var currentRole = roles.FirstOrDefault(role => role?.Company?.Id == companyId?.ToString());
                    if (currentRole == null) return null;

                    var cachedItem = await _accessManagementCacheManager.GetAccessManagementCacheItem();
                    var portalRole = cachedItem.PortalRolesList.FirstOrDefault(role => role?.Id == currentRole?.PortalRole?.Id);
                    if (portalRole == null) return null;

                    if (portalRole.Department != null)
                    {
                        return new EntityReferenceDto
                        {
                            Id = portalRole.Department?.Id,
                            Name = portalRole.Department?.Name
                        };
                    }
                }
            }

            return null;
        }

        public async Task<bool> PinCompany(Guid companyToPin, bool isPin,int areaType)
        {
            var orgContext = new OrganizationServiceContext(_crmService.GetInstance());
            var contactId = _sessionService.GetContactId();
            var companyId = _sessionService.GetCompanyId();
            var configs = _portalConfigAppService.RetrievePortalConfiguration(new List<string> { PortalConfigurations.MaxPInContact });
            var pinNumber = Convert.ToInt32(configs.Single(a => a.Key == PortalConfigurations.MaxPInContact).Value);
            var result = GetPortalPins(contactId, companyId, areaType, orgContext);

            if (result.Count == pinNumber && isPin)
            {
                throw new UserFriendlyException("TheUserShouldBeAbleToPinUpToItems", pinNumber.ToString());
            }

            var oPortalPin = result.FirstOrDefault(x => x.CompanyPinnedId != null && x.CompanyPinnedId.Id == companyToPin.ToString());
            if (isPin && oPortalPin == null)
            {

                Entity PortalPinEntity = new Entity(EntityNames.PortalPinned);

                PortalPinEntity["pwc_name"] = contactId;
                PortalPinEntity["pwc_useridid"] = new EntityReference(EntityNames.Contact, new Guid(contactId));
                PortalPinEntity["pwc_companyidid"] = new EntityReference(EntityNames.Account, new Guid(companyId));
                PortalPinEntity["pwc_contactid"] = new EntityReference(EntityNames.Contact, new Guid(contactId));
                PortalPinEntity["pwc_company"] = new EntityReference(EntityNames.Account, companyToPin);
                PortalPinEntity["pwc_areatype"] = new OptionSetValue(areaType); // 0 = Announced(PIF Companies) , 1= Subsidiary
                orgContext.AddObject(PortalPinEntity);

                orgContext.SaveChanges();
            }
            else if (!isPin && oPortalPin != null)
            {
                _crmService.Delete(oPortalPin.Id.ToString(), EntityNames.PortalPinned);
            }
            else if (!isPin && oPortalPin == null)
            {
                throw new UserFriendlyException("TheItemIsNotPinned");
            }
            else
            {
                throw new UserFriendlyException("TheItemAlreadyPinned");
            }
            return true;
        }

        private List<PortalPinDto> GetPortalPins(string contactId, string companyId, int areaType, OrganizationServiceContext orgContext)
        {
            IQueryable<Entity> Query = orgContext.CreateQuery(EntityNames.PortalPinned).AsQueryable();
            Query = Query.Where(x => ((Guid)x["pwc_useridid"]).Equals(contactId) && ((Guid)x["pwc_companyidid"]).Equals(companyId) && ((Guid)x["pwc_company"]) != null && (int)x["pwc_areatype"] == areaType);
            var selectQuery = Query.Select(x => new PortalPinDto
            {
                Id = x.Id,
                UserId = CRMOperations.GetValueByAttributeName<EntityReferenceDto>(x, "pwc_useridid", ""),
                CompanyId = CRMOperations.GetValueByAttributeName<EntityReferenceDto>(x, "pwc_companyidid", ""),
                ContactId = CRMOperations.GetValueByAttributeName<EntityReferenceDto>(x, "pwc_contactid", ""),
                KnowledgeItemId = CRMOperations.GetValueByAttributeName<EntityReferenceDto>(x, "pwc_knowledgeitem", ""),
                CompanyPinnedId = CRMOperations.GetValueByAttributeName<EntityReferenceDto>(x, "pwc_company", ""),
            }).ToList();
            return selectQuery;
        }

        private string GetCustomerAddressByCompanyId(Guid companyId)
        {
            var query = new QueryExpression(EntityNames.CustomerAddress)
            {
                ColumnSet = new ColumnSet("parentid", "addresstypecode", "line1", "line2", "city", "stateorprovince", "addresstypecode", "createdon"),
            };
            query.Criteria.AddCondition("parentid", ConditionOperator.Equal, companyId);
            var entityCollection = _crmService.GetInstance().RetrieveMultiple(query);
            var customerAddressList = entityCollection.Entities.Select(FillCustomerAddress).ToList();
            if (customerAddressList.Any())
            {
                // Find the preferred address by type or by most recent date
                var oCustomerAddress = customerAddressList
                    .Where(x => x.AddressType !=null && x.AddressType.Value == "3" /*Primary*/)
                    .OrderByDescending(z => z.CreationDate)
                    .FirstOrDefault() ??
                    customerAddressList.OrderByDescending(z => z.CreationDate).FirstOrDefault();

                if (oCustomerAddress != null)
                {
                    var addressParts = new List<string>();

                    // Add each address component if it is not empty or null
                    if (!string.IsNullOrWhiteSpace(oCustomerAddress.Line1))
                        addressParts.Add(oCustomerAddress.Line1);
                    if (!string.IsNullOrWhiteSpace(oCustomerAddress.Line2))
                        addressParts.Add(oCustomerAddress.Line2);

                    // Add City with "/" if it's present
                    if (!string.IsNullOrWhiteSpace(oCustomerAddress.City))
                        addressParts.Add($"/ {oCustomerAddress.City}");

                    // Add State with "/" if it's present
                    if (!string.IsNullOrWhiteSpace(oCustomerAddress.State))
                        addressParts.Add($"/ {oCustomerAddress.State}");

                    return string.Join(" ", addressParts).Trim();
                }
            }
            return string.Empty;
        }
        private CustomerAddress FillCustomerAddress(Entity entity)
        {
            return new CustomerAddress
            {
                Id = entity.Id.ToString(),
                Line1 = entity.GetValueByAttributeName<string>("line1"),
                Line2 =  entity.GetValueByAttributeName<string>("line2"),
                City = entity.GetValueByAttributeName<string>("city"),
                State = entity.GetValueByAttributeName<string>("stateorprovince"),
                CreationDate = entity.GetValueByAttributeName<DateTime>("createdon"),
                AddressType = entity.GetValueByAttributeName<EntityOptionSetDto>("addresstypecode"),
            };
        }

        public async Task<string> GetCompanyNameById(string companyId)
        {
            try
            {
                var companyEntity = _crmService.GetById(EntityNames.Account,
                       new[] { "accountid", "name" }, Guid.Parse(companyId), "accountid");

                return companyEntity.GetValueByAttributeName<string>("name");
            }catch(Exception)
            {
                throw new UserFriendlyException("CompanyNotFound");
            }
        }

        private List<Guid> GetMembershipTypes(List<int> codes)
        {
            var query = new QueryExpression(EntityNames.MembershipType)
            {
                ColumnSet = new ColumnSet("ntw_membershiptypeid", "ntw_code")
            };
            if (codes.Any())
            {
                var codeObjects = codes.Cast<object>().ToArray();
                query.Criteria.AddCondition("ntw_code", ConditionOperator.In, codeObjects);
            }

            var entityCollection = _crmService.GetInstance().RetrieveMultiple(query);

            return entityCollection.Entities.Select(x => x.Id).ToList();
        }
    }
}
