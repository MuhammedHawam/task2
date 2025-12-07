using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using PIF.EBP.Application.Cache;
using PIF.EBP.Application.DocumentLocation;
using PIF.EBP.Application.EntitiesCache;
using PIF.EBP.Application.KnowledgeHub.DTOs;
using PIF.EBP.Application.MetaData.DTOs;
using PIF.EBP.Application.RuleBook.DTOs;
using PIF.EBP.Application.Shared;
using PIF.EBP.Core.CRM;
using PIF.EBP.Core.FileManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Services.Description;
using static PIF.EBP.Application.Shared.Enums;


namespace PIF.EBP.Application.RuleBook.Implementation
{
    public class RuleBookAppService : IRuleBookAppService
    {
        private readonly ICrmService _crmService;
        private readonly IEntitiesCacheAppService _entitiesCacheAppService;
        private readonly ICacheManager _cacheManager;
        private readonly IDocumentLocationAppService _documentLocationAppService;
        private readonly IFileManagement _fileService;

        public RuleBookAppService(ICrmService crmService,
         IEntitiesCacheAppService entitiesCacheAppService,
         IDocumentLocationAppService documentLocationAppService,
         IFileManagement fileService,
         ICacheManager cacheManager
         )
        {
            _crmService = crmService;
            _entitiesCacheAppService = entitiesCacheAppService;
            _cacheManager = cacheManager;
            _documentLocationAppService = documentLocationAppService;
            _fileService = fileService;
        }

        //RuleBook with chapters retrieve 
        public async Task<RuleBookResponse> RetrieveRuleBookList()
        {
            const string cacheKey = "RuleBookList";
            var cached = await _cacheManager.GetObjectFromCacheAsync<RuleBookResponse>(cacheKey);
            if (cached != null)
                return cached;

            var response = new RuleBookResponse
            {
                RuleBookList = new List<RuleBookDto>()
            };

            var query = BuildRuleBookQuery();
            var ruleBookEntities = _crmService.GetInstance().RetrieveMultiple(query);
            var ruleBooks = MapRuleBooksWithChapters(ruleBookEntities.Entities);
            response.RuleBookList = ruleBooks;

            await _cacheManager.SetObjectInCacheAsync(cacheKey, response, TimeSpan.FromHours(2500));

            return response;
        }
        private QueryExpression BuildRuleBookQuery()
        {
            var query = new QueryExpression(EntityNames.RuleBook)
            {
                ColumnSet = new ColumnSet(
                    "pwc_rulebookid",
                    "pwc_name",
                    "pwc_namear",
                    "pwc_rulebooktypetypecode",
                    "pwc_rulebookstatustypecode"
                ),
                Criteria = new FilterExpression
                {
                    Conditions =
            {
                new ConditionExpression("pwc_rulebooktypetypecode", ConditionOperator.Equal, (int)RuleBookType.PC),
                new ConditionExpression("pwc_rulebookstatustypecode", ConditionOperator.Equal, (int)RuleBookStatus.Published)
            }
                }
            };

            var chapterLink = query.AddLink("pwc_chapter", "pwc_rulebookid", "pwc_rulebookid", JoinOperator.LeftOuter);
            chapterLink.Columns = new ColumnSet("pwc_name", "pwc_namear", "pwc_number", "pwc_chapterid");
            chapterLink.EntityAlias = "chapter";

            return query;
        }
        private List<RuleBookDto> MapRuleBooksWithChapters(IEnumerable<Entity> entities)
        {
            return entities
                .GroupBy(e => e.GetAttributeValue<Guid>("pwc_rulebookid"))
                .Select(group =>
                {
                    var mainEntity = group.First();

                    var chapters = group
                        .Where(e => e.Contains("chapter.pwc_chapterid"))
                        .Select(e => new ChapterDto
                        {
                            Id = e.GetAttributeValue<AliasedValue>("chapter.pwc_chapterid")?.Value?.ToString() ?? string.Empty,
                            Title = e.GetAttributeValue<AliasedValue>("chapter.pwc_name")?.Value as string ?? string.Empty,
                            TitleAr = e.GetAttributeValue<AliasedValue>("chapter.pwc_namear")?.Value as string ?? string.Empty,
                            Number = e.GetAttributeValue<AliasedValue>("chapter.pwc_number")?.Value?.ToString() ?? string.Empty
                        })
                        .OrderBy(c => int.TryParse(c.Number, out var num) ? num : int.MaxValue)
                        .ToList();

                    return new RuleBookDto
                    {
                        Id = mainEntity.Id,
                        Name = CRMUtility.GetAttributeValue(mainEntity, "pwc_name", string.Empty),
                        NameAr = CRMUtility.GetAttributeValue(mainEntity, "pwc_namear", string.Empty),
                        Description = CRMUtility.GetAttributeValue(mainEntity, "pwc_description", string.Empty),
                        Chapters = chapters
                    };
                })
                .ToList();
        }

        //Sections with sub sections retrieve by chapter id
        public async Task<SectionResponse> RetrieveSectionsByChapter(SectionRequest oSectionRequest)
        {
            string cacheKey = $"SectionResponse_{oSectionRequest.chapterId}_PageNo{oSectionRequest.PagingRequest.PageNo}_PageSi{oSectionRequest.PagingRequest.PageSize}";
            var cached = await _cacheManager.GetObjectFromCacheAsync<SectionResponse>(cacheKey);
            if (cached != null)
                return cached;

            var response = new SectionResponse();

            var query = BuildSectionQuery(oSectionRequest);

            var sectionEntities = _crmService.GetInstance().RetrieveMultiple(query);

            response.Sections = MapSectionsWithSubSections(sectionEntities.Entities);

            response.TotalCount = sectionEntities.TotalRecordCount;


            await _cacheManager.SetObjectInCacheAsync(cacheKey, response, TimeSpan.FromHours(2500));

            return response;
        }
        private QueryExpression BuildSectionQuery(SectionRequest oSectionRequest)
        {
            var query = new QueryExpression("pwc_section")
            {
                ColumnSet = new ColumnSet("pwc_name", "pwc_namear", "pwc_number", "pwc_sectionid", "pwc_chapterid"),
                Criteria = new FilterExpression
                {
                    Conditions =
            {
                new ConditionExpression("pwc_chapterid", ConditionOperator.Equal, oSectionRequest.chapterId),
                new ConditionExpression("pwc_sectionstatustypecode", ConditionOperator.Equal, (int)RuleBookStatus.Published)

            }
                },


                PageInfo = new PagingInfo
                {
                    PageNumber = oSectionRequest.PagingRequest.PageNo,
                    Count = oSectionRequest.PagingRequest.PageSize,
                    PagingCookie = null,
                    ReturnTotalRecordCount = true
                }

            };

            var subSectionLink = query.AddLink("pwc_subsection", "pwc_sectionid", "pwc_sectionid", JoinOperator.LeftOuter);
            subSectionLink.Columns = new ColumnSet("pwc_name", "pwc_namear", "pwc_description", "pwc_descriptionar", "pwc_number", "pwc_sectionid", "pwc_subsectionid", "modifiedon");
            subSectionLink.LinkCriteria = new FilterExpression
            {
                Conditions =
                              {
                                  new ConditionExpression("pwc_subsectionstatustypecode", ConditionOperator.Equal, (int)RuleBookStatus.Published)
                              }
            };

            subSectionLink.EntityAlias = "sub";

            return query;
        }
        private List<SectionDto> MapSectionsWithSubSections(IEnumerable<Entity> entities)
        {
            return entities
                .GroupBy(e => e.GetAttributeValue<Guid>("pwc_sectionid"))
                .Select(group =>
                {
                    var sectionEntity = group.First();

                    var subSections = group
                        .Where(e => e.Contains("sub.pwc_subsectionid"))
                        .Select(e => new SubSectionDto
                        {
                            Id = e.GetAttributeValue<AliasedValue>("sub.pwc_subsectionid")?.Value?.ToString() ?? string.Empty,
                            Name = e.GetAttributeValue<AliasedValue>("sub.pwc_name")?.Value as string ?? string.Empty,
                            NameAr = e.GetAttributeValue<AliasedValue>("sub.pwc_namear")?.Value as string ?? string.Empty,
                            Description = e.GetAttributeValue<AliasedValue>("sub.pwc_description")?.Value as string ?? string.Empty,
                            DescriptionAr = e.GetAttributeValue<AliasedValue>("sub.pwc_descriptionar")?.Value as string ?? string.Empty,
                            Number = e.GetAttributeValue<AliasedValue>("sub.pwc_number")?.Value?.ToString() ?? string.Empty,
                            LastUpdatedOn = (DateTime)e.GetAttributeValue<AliasedValue>("sub.modifiedon")?.Value
                        })
                        .OrderBy(s => int.TryParse(s.Number, out var num) ? num : int.MaxValue)
                        .ToList();

                    return new SectionDto
                    {
                        Name = sectionEntity.GetAttributeValue<string>("pwc_name") ?? string.Empty,
                        NameAr = sectionEntity.GetAttributeValue<string>("pwc_namear") ?? string.Empty,
                        Number = sectionEntity.GetAttributeValue<string>("pwc_number") ?? string.Empty,
                        SubSections = subSections
                    };
                })
                .OrderBy(s => int.TryParse(s.Number, out var num) ? num : int.MaxValue)
                .ToList();
        }

        //SearchBox Dropdown Values
        public async Task<SearchDropdownReponse> RetrieveDropDownLists()
        {
            var response = new SearchDropdownReponse
            {
                PCLifeCycleStage = new List<EntityOptionSetDto>(),
                TypeOfRequirment = new List<EntityOptionSetDto>(),
                Theme = new List<EntityOptionSetDto>()
            };

            response.PCLifeCycleStage = _entitiesCacheAppService.RetrieveOptionSetCacheByKey(OptionSetKey.PcStage) ?? new List<EntityOptionSetDto>();
            response.TypeOfRequirment = _entitiesCacheAppService.RetrieveOptionSetCacheByKey(OptionSetKey.TypeOfRequirments) ?? new List<EntityOptionSetDto>();
            response.Theme = _entitiesCacheAppService.RetrieveOptionSetCacheByKey(OptionSetKey.Theme) ?? new List<EntityOptionSetDto>();

            return await Task.FromResult(response);
        }

        //Rules list from subsection guid
        public async Task<RulesReponse> RetrieveRulesBySubSection(RulesRequest oRuleRequest)
        {
            RulesReponse cached = null;

            if (string.IsNullOrWhiteSpace(oRuleRequest.SearchKeyword))
            {
                string cacheKey = $"RulesBySubSection_{oRuleRequest.SubSectionId}_Page{oRuleRequest.PagingRequest.PageNo}_Size{oRuleRequest.PagingRequest.PageSize}_{oRuleRequest.PagingRequest.SortOrder}_Field{oRuleRequest.PagingRequest.SortField}";
                cached = await _cacheManager.GetObjectFromCacheAsync<RulesReponse>(cacheKey);
                if (cached != null)
                    return cached;
            }

            var service = _crmService.GetInstance();
            var query = BuildRulesBySubSectionQuery(oRuleRequest);
            var result = service.RetrieveMultiple(query);

            var rules = MapRuleEntities(result.Entities);

            var location = result.Entities.Any()
                ? MapRuleLocation(result.Entities.FirstOrDefault())
                : await RetrieveLocationBySubSectionId(oRuleRequest.SubSectionId);

            var totalCount = result.TotalRecordCount;

            var response = new RulesReponse
            {
                RuleList = rules,
                Location = location,
                TotalCount = totalCount
            };

            if (string.IsNullOrWhiteSpace(oRuleRequest.SearchKeyword))
            {
                string cacheKey = $"RulesBySubSection_{oRuleRequest.SubSectionId}_Page{oRuleRequest.PagingRequest.PageNo}_Size{oRuleRequest.PagingRequest.PageSize}_{oRuleRequest.PagingRequest.SortOrder}";
                await _cacheManager.SetObjectInCacheAsync(cacheKey, response, TimeSpan.FromHours(2500));
            }

            return response;
        }
        private QueryExpression BuildRulesBySubSectionQuery(RulesRequest oRuleRequest)
        {
            var query = new QueryExpression("pwc_rule")
            {
                ColumnSet = new ColumnSet("pwc_name", "pwc_namear", "pwc_number", "pwc_ruleid"),
                Criteria = new FilterExpression
                {
                    Conditions =
            {
                new ConditionExpression("pwc_subsectionid", ConditionOperator.Equal, oRuleRequest.SubSectionId),
                new ConditionExpression("pwc_rulestatustypecode", ConditionOperator.Equal, (int)RuleBookStatus.Published)

            }
                },

                PageInfo = new PagingInfo
                {
                    PageNumber = oRuleRequest.PagingRequest.PageNo,
                    Count = oRuleRequest.PagingRequest.PageSize,
                    PagingCookie = null,
                    ReturnTotalRecordCount = true
                }
            };

            if (!string.IsNullOrWhiteSpace(oRuleRequest.SearchKeyword))
            {
                var searchFilter = new FilterExpression(LogicalOperator.Or);
                searchFilter.Conditions.Add(new ConditionExpression("pwc_name", ConditionOperator.Like, $"%{oRuleRequest.SearchKeyword}%"));
                searchFilter.Conditions.Add(new ConditionExpression("pwc_namear", ConditionOperator.Like, $"%{oRuleRequest.SearchKeyword}%"));
                query.Criteria.Filters.Add(searchFilter);
            }

            if (!string.IsNullOrWhiteSpace(oRuleRequest.PagingRequest.SortField))
            {
                query.Orders.Add(new OrderExpression(oRuleRequest.PagingRequest.SortField, (OrderType)oRuleRequest.PagingRequest.SortOrder));
            }

            var subSectionLink = query.AddLink("pwc_subsection", "pwc_subsectionid", "pwc_subsectionid", JoinOperator.LeftOuter);
            subSectionLink.EntityAlias = "subsection";
            subSectionLink.Columns = new ColumnSet("pwc_name", "pwc_namear", "pwc_sectionid");

            var sectionLink = subSectionLink.AddLink("pwc_section", "pwc_sectionid", "pwc_sectionid", JoinOperator.LeftOuter);
            sectionLink.EntityAlias = "section";
            sectionLink.Columns = new ColumnSet("pwc_name", "pwc_namear", "pwc_chapterid");

            var chapterLink = sectionLink.AddLink("pwc_chapter", "pwc_chapterid", "pwc_chapterid", JoinOperator.LeftOuter);
            chapterLink.EntityAlias = "chapter";
            chapterLink.Columns = new ColumnSet("pwc_name", "pwc_namear");

            return query;
        }
        private List<RuleDto> MapRuleEntities(IEnumerable<Entity> entities)
        {
            return entities.Select(e => new RuleDto
            {
                Title = e.GetAttributeValue<string>("pwc_name") ?? string.Empty,
                TitleAr = e.GetAttributeValue<string>("pwc_namear") ?? string.Empty,
                Number = e.GetAttributeValue<string>("pwc_number") ?? string.Empty,
                Id = e.Id.ToString()
            }).ToList();
        }
        private RuleLocationDto MapRuleLocation(Entity ruleEntity)
        {
            if (ruleEntity == null) return new RuleLocationDto();

            return new RuleLocationDto
            {
                SubSectionName = ruleEntity.GetAttributeValue<AliasedValue>("subsection.pwc_name")?.Value as string ?? string.Empty,
                SubSectionNameAr = ruleEntity.GetAttributeValue<AliasedValue>("subsection.pwc_namear")?.Value as string ?? string.Empty,
                SectionName = ruleEntity.GetAttributeValue<AliasedValue>("section.pwc_name")?.Value as string ?? string.Empty,
                SectionNameAr = ruleEntity.GetAttributeValue<AliasedValue>("section.pwc_namear")?.Value as string ?? string.Empty,
                ChapterName = ruleEntity.GetAttributeValue<AliasedValue>("chapter.pwc_name")?.Value as string ?? string.Empty,
                ChapterNameAr = ruleEntity.GetAttributeValue<AliasedValue>("chapter.pwc_namear")?.Value as string ?? string.Empty

            };
        }
        private async Task<RuleLocationDto> RetrieveLocationBySubSectionId(Guid subSectionId)
        {
            var service = _crmService.GetInstance();

            var query = new QueryExpression("pwc_subsection")
            {
                ColumnSet = new ColumnSet("pwc_name", "pwc_namear", "pwc_subsectionid"),
                Criteria = new FilterExpression
                {
                    Conditions =
            {
                new ConditionExpression("pwc_subsectionid", ConditionOperator.Equal, subSectionId)
            }
                }
            };

            // Link to Section
            var sectionLink = query.AddLink("pwc_section", "pwc_sectionid", "pwc_sectionid", JoinOperator.LeftOuter);
            sectionLink.EntityAlias = "section";
            sectionLink.Columns = new ColumnSet("pwc_name", "pwc_namear", "pwc_chapterid");

            // Link to Chapter
            var chapterLink = sectionLink.AddLink("pwc_chapter", "pwc_chapterid", "pwc_chapterid", JoinOperator.LeftOuter);
            chapterLink.EntityAlias = "chapter";
            chapterLink.Columns = new ColumnSet("pwc_name", "pwc_namear");

            var result = service.RetrieveMultiple(query);
            var subsectionEntity = result.Entities.FirstOrDefault();
            if (subsectionEntity == null)
                return null;

            var location = new RuleLocationDto
            {
                SubSectionName = subsectionEntity.GetAttributeValue<string>("pwc_name") ?? string.Empty,
                SubSectionNameAr = subsectionEntity.GetAttributeValue<string>("pwc_namear") ?? string.Empty,
                SectionName = subsectionEntity.GetAttributeValue<AliasedValue>("section.pwc_name")?.Value as string ?? string.Empty,
                SectionNameAr = subsectionEntity.GetAttributeValue<AliasedValue>("section.pwc_namear")?.Value as string ?? string.Empty,
                ChapterName = subsectionEntity.GetAttributeValue<AliasedValue>("chapter.pwc_name")?.Value as string ?? string.Empty,
                ChapterNameAr = subsectionEntity.GetAttributeValue<AliasedValue>("chapter.pwc_namear")?.Value as string ?? string.Empty
            };

            return location;
        }

        //Sub Rules list from rule guid
        public async Task<SubRulesResponse> RetrieveSubRulesWithRuleDetails(Guid ruleId)
        {
            string cacheKey = $"SubRulesWithRuleDetails_{ruleId}";
            var cached = await _cacheManager.GetObjectFromCacheAsync<SubRulesResponse>(cacheKey);
            if (cached != null)
                return cached;

            var service = _crmService.GetInstance();

            var ruleEntity = RetrieveRuleEntity(ruleId);
            var subRuleEntities = service.RetrieveMultiple(BuildSubRulesQuery(ruleId));

            var response = new SubRulesResponse
            {
                Rule = MapRuleToDetailsDto(ruleEntity),
                SubRuleList = MapSubRules(subRuleEntities.Entities)
            };

            await _cacheManager.SetObjectInCacheAsync(cacheKey, response, TimeSpan.FromHours(2500));

            return response;
        }
        private QueryExpression BuildSubRulesQuery(Guid ruleId)
        {
            return new QueryExpression("pwc_subrule")
            {
                ColumnSet = new ColumnSet("pwc_description", "pwc_descriptionar", "pwc_number","pwc_name","pwc_namear"),
                Criteria = new FilterExpression
                {
                    Conditions =
            {
                new ConditionExpression("pwc_ruleid", ConditionOperator.Equal, ruleId)
            }
                },
                Orders =
        {
            new OrderExpression("pwc_number", OrderType.Ascending)
        }
            };
        }
        private Entity RetrieveRuleEntity(Guid ruleId)
        {
            var query = new QueryExpression("pwc_rule")
            {
                ColumnSet = new ColumnSet(
                    "modifiedon",
                    "pwc_typeofrequirementtypecode",
                    "pwc_pclifecyclestagetypecode",
                    "pwc_themetypecode",
                    "pwc_description",
                    "pwc_descriptionar",
                    "pwc_name",
                    "pwc_namear",
                    "pwc_ruleid"
                ),
                Criteria = new FilterExpression
                {
                    Conditions =
            {
                new ConditionExpression("pwc_ruleid", ConditionOperator.Equal, ruleId)
            }
                }
            };

            // Link: pwc_rule → pwc_subsection
            var subSectionLink = query.AddLink("pwc_subsection", "pwc_subsectionid", "pwc_subsectionid", JoinOperator.LeftOuter);
            subSectionLink.EntityAlias = "subsection";
            subSectionLink.Columns = new ColumnSet("pwc_name", "pwc_namear", "pwc_sectionid", "pwc_subsectionid");

            // Link: pwc_subsection → pwc_section
            var sectionLink = subSectionLink.AddLink("pwc_section", "pwc_sectionid", "pwc_sectionid", JoinOperator.LeftOuter);
            sectionLink.EntityAlias = "section";
            sectionLink.Columns = new ColumnSet("pwc_name", "pwc_namear", "pwc_chapterid");

            // Link: pwc_section → pwc_chapter
            var chapterLink = sectionLink.AddLink("pwc_chapter", "pwc_chapterid", "pwc_chapterid", JoinOperator.LeftOuter);
            chapterLink.EntityAlias = "chapter";
            chapterLink.Columns = new ColumnSet("pwc_name", "pwc_namear");

            return _crmService.GetInstance().RetrieveMultiple(query).Entities.FirstOrDefault();
        }
        private RuleDetailsDto MapRuleToDetailsDto(Entity ruleEntity)
        {
            var typeOfReqVal = ruleEntity.GetAttributeValue<OptionSetValue>("pwc_typeofrequirementtypecode")?.Value;
            var pcStageVal = ruleEntity.GetAttributeValue<OptionSetValue>("pwc_pclifecyclestagetypecode")?.Value;
            var themeVal = ruleEntity.GetAttributeValue<OptionSetValue>("pwc_themetypecode")?.Value;

            var typeOfRequirementLabel = _entitiesCacheAppService
                .RetrieveOptionSetCacheByKey(OptionSetKey.TypeOfRequirments)
                ?.FirstOrDefault(o => o.Value == typeOfReqVal.ToString())?.Name ?? string.Empty;

            var typeOfRequirementLabelAr = _entitiesCacheAppService
               .RetrieveOptionSetCacheByKey(OptionSetKey.TypeOfRequirments)
               ?.FirstOrDefault(o => o.Value == typeOfReqVal.ToString())?.NameAr ?? string.Empty;

            var pcStageLabel = _entitiesCacheAppService
                .RetrieveOptionSetCacheByKey(OptionSetKey.PcStage)
                ?.FirstOrDefault(o => o.Value == pcStageVal.ToString())?.Name ?? string.Empty;
            var pcStageLabelAr = _entitiesCacheAppService
                .RetrieveOptionSetCacheByKey(OptionSetKey.PcStage)
                ?.FirstOrDefault(o => o.Value == pcStageVal.ToString())?.NameAr ?? string.Empty;
            var themeLabel = _entitiesCacheAppService
               .RetrieveOptionSetCacheByKey(OptionSetKey.Theme)
               ?.FirstOrDefault(o => o.Value == themeVal.ToString())?.Name ?? string.Empty;
            var themeLabelAr = _entitiesCacheAppService
                .RetrieveOptionSetCacheByKey(OptionSetKey.Theme)
                ?.FirstOrDefault(o => o.Value == themeVal.ToString())?.NameAr ?? string.Empty;
            return new RuleDetailsDto
            {
                ModifiedOn = ruleEntity.GetAttributeValue<DateTime?>("modifiedon"),
                Theme = themeLabel,
                ThemeAr = themeLabelAr,
                TypeOfRequirementLabel = typeOfRequirementLabel,
                TypeOfRequirementLabelAr = typeOfRequirementLabelAr,
                PcLifeCycleStageLabel = pcStageLabel,
                PcLifeCycleStageLabelAr = pcStageLabelAr,
                SubSectionId = (ruleEntity.GetAttributeValue<AliasedValue>("subsection.pwc_subsectionid")?.Value as Guid?) ?? Guid.Empty,
                SubSectionName = ruleEntity.GetAttributeValue<AliasedValue>("subsection.pwc_name")?.Value as string ?? string.Empty,
                SubSectionNameAr = ruleEntity.GetAttributeValue<AliasedValue>("subsection.pwc_namear")?.Value as string ?? string.Empty,
                SectionName = ruleEntity.GetAttributeValue<AliasedValue>("section.pwc_name")?.Value as string ?? string.Empty,
                SectionNameAr = ruleEntity.GetAttributeValue<AliasedValue>("section.pwc_namear")?.Value as string ?? string.Empty,
                ChapterName = ruleEntity.GetAttributeValue<AliasedValue>("chapter.pwc_name")?.Value as string ?? string.Empty,
                ChapterNameAr = ruleEntity.GetAttributeValue<AliasedValue>("chapter.pwc_namear")?.Value as string ?? string.Empty,
                Description = ruleEntity.GetAttributeValue<string>("pwc_description") ?? string.Empty,
                DescriptionAr = ruleEntity.GetAttributeValue<string>("pwc_descriptionar") ?? string.Empty,
                Name = ruleEntity.GetAttributeValue<string>("pwc_name") ?? string.Empty,
                NameAr = ruleEntity.GetAttributeValue<string>("pwc_namear") ?? string.Empty,
            };
        }
        private List<SubRuleDto> MapSubRules(IEnumerable<Entity> entities)
        {
            return entities.Select(e => new SubRuleDto
            {
                Id = e.Id,
                Name = e.GetAttributeValue<string>("pwc_name") ?? string.Empty,
                NameAr = e.GetAttributeValue<string>("pwc_namear") ?? string.Empty,
                Description = e.GetAttributeValue<string>("pwc_description") ?? string.Empty,
                DescriptionAr = e.GetAttributeValue<string>("pwc_descriptionar") ?? string.Empty
            }).ToList();
        }
        //Search Rules 
        public async Task<PagedRuleSearchResult> SearchRulesAsync(RuleSearchRequest request)
        {

            var query = BuildSearchRulesQuery(request);
            var pagedEntities = _crmService.GetInstance().RetrieveMultiple(query);

            var mapped = MapRulesToSearchResult(pagedEntities.Entities, request);
            var totalCount = pagedEntities.TotalRecordCount;


            return new PagedRuleSearchResult
            {
                Items = mapped,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
            };
        }
        private QueryExpression BuildSearchRulesQuery(RuleSearchRequest request)
        {
            var query = new QueryExpression("pwc_rule")
            {
                ColumnSet = new ColumnSet(
                    "pwc_ruleid", "pwc_name", "pwc_namear", "pwc_description", "pwc_descriptionar",
                    "modifiedon", "pwc_pclifecyclestagetypecode", "pwc_typeofrequirementtypecode", "pwc_themetypecode", "pwc_rulestatustypecode"
                ),
                Criteria = BuildCommonFilters(request),
                PageInfo = new PagingInfo
                {
                    PageNumber = request.PageNumber,
                    Count = request.PageSize,
                    PagingCookie = null,
                    ReturnTotalRecordCount = true
                }
            };

            // Add links: rule → subsection → section → chapter
            var subSectionLink = query.AddLink("pwc_subsection", "pwc_subsectionid", "pwc_subsectionid", JoinOperator.Inner);
            subSectionLink.EntityAlias = "sub";
            subSectionLink.LinkCriteria = new FilterExpression(LogicalOperator.And);
            subSectionLink.LinkCriteria.AddCondition("pwc_subsectionid", ConditionOperator.NotNull);
            subSectionLink.Columns = new ColumnSet("pwc_subsectionid");

            var sectionLink = subSectionLink.AddLink("pwc_section", "pwc_sectionid", "pwc_sectionid", JoinOperator.Inner);
            sectionLink.EntityAlias = "sec";
            sectionLink.LinkCriteria = new FilterExpression(LogicalOperator.And);
            sectionLink.LinkCriteria.AddCondition("pwc_sectionid", ConditionOperator.NotNull);
            sectionLink.Columns = new ColumnSet("pwc_sectionid");

            var chapterLink = sectionLink.AddLink("pwc_chapter", "pwc_chapterid", "pwc_chapterid", JoinOperator.Inner);
            chapterLink.EntityAlias = "chap";
            chapterLink.Columns = new ColumnSet("pwc_name", "pwc_namear", "pwc_chapterid");

            if (request.ChapterId.HasValue)
            {
                chapterLink.LinkCriteria = new FilterExpression(LogicalOperator.And);
                chapterLink.LinkCriteria.AddCondition("pwc_chapterid", ConditionOperator.NotNull);
                chapterLink.LinkCriteria.AddCondition("pwc_chapterid", ConditionOperator.Equal, request.ChapterId);
            }

            return query;
        }
        private FilterExpression BuildCommonFilters(RuleSearchRequest request)
        {
            var filter = new FilterExpression(LogicalOperator.And);
            filter.AddCondition("pwc_rulestatustypecode", ConditionOperator.Equal, (int)RuleBookStatus.Published);

            // Determine field names based on locale
            string titleField = request.Locale == "ar" ? "pwc_namear" : "pwc_name";
            string descriptionField = request.Locale == "ar" ? "pwc_descriptionar" : "pwc_description";

            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var keywordFilter = new FilterExpression(LogicalOperator.Or);
                keywordFilter.AddCondition(titleField, ConditionOperator.Like, $"%{request.Keyword}%");
                keywordFilter.AddCondition(descriptionField, ConditionOperator.Like, $"%{request.Keyword}%");
                filter.AddFilter(keywordFilter);
            }

            if (!string.IsNullOrWhiteSpace(request.RuleBookId))
                filter.AddCondition("pwc_rulebookid", ConditionOperator.Equal, request.RuleBookId);

            if (request.Stage.HasValue)
                filter.AddCondition("pwc_pclifecyclestagetypecode", ConditionOperator.Equal, request.Stage);

            if (request.TypeOfRequirement.HasValue)
                filter.AddCondition("pwc_typeofrequirementtypecode", ConditionOperator.Equal, request.TypeOfRequirement);

            if (request.Theme.HasValue)
                filter.AddCondition("pwc_themetypecode", ConditionOperator.Equal, request.Theme);

            if (request.LastUpdatedFrom.HasValue && request.LastUpdatedTo.HasValue)
            {
                var dateRangeFilter = new FilterExpression(LogicalOperator.And);
                dateRangeFilter.AddCondition("modifiedon", ConditionOperator.OnOrAfter, request.LastUpdatedFrom.Value);
                dateRangeFilter.AddCondition("modifiedon", ConditionOperator.OnOrBefore, request.LastUpdatedTo.Value);
                filter.AddFilter(dateRangeFilter);
            }
            else if (request.LastUpdatedFrom.HasValue)
            {
                filter.AddCondition("modifiedon", ConditionOperator.OnOrAfter, request.LastUpdatedFrom.Value);
            }
            else if (request.LastUpdatedTo.HasValue)
            {
                filter.AddCondition("modifiedon", ConditionOperator.OnOrBefore, request.LastUpdatedTo.Value);
            }

            return filter;
        }
        private List<RuleSearchResultDto> MapRulesToSearchResult(IEnumerable<Entity> entities, RuleSearchRequest request)
        {
            var pcStages = _entitiesCacheAppService.RetrieveOptionSetCacheByKey(OptionSetKey.PcStage);
            var reqTypes = _entitiesCacheAppService.RetrieveOptionSetCacheByKey(OptionSetKey.TypeOfRequirments);
            var themeTypes = _entitiesCacheAppService.RetrieveOptionSetCacheByKey(OptionSetKey.Theme);

            return entities
                .GroupBy(e => e.Id)
                .Select(group =>
                {
                    var main = group.First();


                    var chapterName = group
                        .Select(e => e.GetAttributeValue<AliasedValue>(
                            request.Locale == "ar" ? "chap.pwc_namear" : "chap.pwc_name")?.Value as string)
                        .FirstOrDefault(name => !string.IsNullOrWhiteSpace(name)) ?? string.Empty;


                    var stageValue = main.GetAttributeValue<OptionSetValue>("pwc_pclifecyclestagetypecode")?.Value;
                    var reqTypeValue = main.GetAttributeValue<OptionSetValue>("pwc_typeofrequirementtypecode")?.Value;
                    var themeValue = main.GetAttributeValue<OptionSetValue>("pwc_themetypecode")?.Value;

                    return new RuleSearchResultDto
                    {
                        Id = main.Id.ToString(),
                        Title = request.Locale == "ar" ? main.GetAttributeValue<string>("pwc_namear") : main.GetAttributeValue<string>("pwc_name"),
                        Description = request.Locale == "ar" ? main.GetAttributeValue<string>("pwc_descriptionar") : main.GetAttributeValue<string>("pwc_description"),
                        ModifiedOn = main.GetAttributeValue<DateTime?>("modifiedon"),
                        Theme = themeTypes?.FirstOrDefault(x => x.Value == themeValue?.ToString())?.Name ?? string.Empty,
                        StageLabel = pcStages?.FirstOrDefault(x => x.Value == stageValue?.ToString())?.Name ?? string.Empty,
                        TypeOfRequirementLabel = reqTypes?.FirstOrDefault(x => x.Value == reqTypeValue?.ToString())?.Name ?? string.Empty,
                        ChapterName = chapterName
                    };
                })
                .ToList();
        }
        // Document List
        public async Task<List<RuleDocumentRsp>> GetRuleAndSubRuleDocuments(Guid ruleId)
        {
            var allDocuments = new List<RuleDocumentRsp>();

            // 1. Retrieve rule + sub-rules
            var ruleWithSubRules = GetRuleWithSubRules(ruleId);
            if (ruleWithSubRules == null) return allDocuments;

            // 2. Main rule documents
            var mainRuleDocs = await RetrieveDocumentsFor(ruleWithSubRules.RuleId, "pwc_rule");
            SetMasterName(mainRuleDocs, ruleWithSubRules.RuleName, ruleWithSubRules.RuleNameAr);
            allDocuments.AddRange(mainRuleDocs);

            // 3. Sub-rule documents
            foreach (var subRule in ruleWithSubRules.SubRules)
            {
                var subRuleDocs = await RetrieveDocumentsFor(subRule.Id, "pwc_subrule");
                SetMasterName(subRuleDocs, subRule.Name, subRule.NameAr);
                allDocuments.AddRange(subRuleDocs);
            }

            return allDocuments;
        }
        // Get rule + sub-rules from CRM
        private RuleWithSubRules GetRuleWithSubRules(Guid ruleId)
        {
            var query = new QueryExpression("pwc_rule")
            {
                ColumnSet = new ColumnSet("pwc_name", "pwc_namear"),
                Criteria = { Conditions = { new ConditionExpression("pwc_ruleid", ConditionOperator.Equal, ruleId) } }
            };

            // Join sub-rules
            var subRuleLink = query.AddLink("pwc_subrule", "pwc_ruleid", "pwc_ruleid", JoinOperator.LeftOuter);
            subRuleLink.Columns = new ColumnSet("pwc_subruleid", "pwc_name", "pwc_namear");
            subRuleLink.EntityAlias = "subRule";

            var result = _crmService.GetInstance().RetrieveMultiple(query);
            var ruleEntity = result.Entities.FirstOrDefault();
            if (ruleEntity == null) return null;

            var subRules = result.Entities
                .Where(e => e.Attributes.ContainsKey("subRule.pwc_subruleid"))
                .Select(e => new SubRuleInfo
                {
                    Id = ((AliasedValue)e["subRule.pwc_subruleid"]).Value is Guid g ? g : Guid.Empty,
                    Name = ((AliasedValue)e["subRule.pwc_name"]).Value as string,
                    NameAr = ((AliasedValue)e["subRule.pwc_namear"]).Value as string
                })
                .Where(sr => sr.Id != Guid.Empty)
                .GroupBy(sr => sr.Id)
                .Select(g => g.First())
                .ToList();

            return new RuleWithSubRules
            {
                RuleId = ruleEntity.Id,
                RuleName = ruleEntity.GetAttributeValue<string>("pwc_name"),
                RuleNameAr = ruleEntity.GetAttributeValue<string>("pwc_namear"),
                SubRules = subRules
            };
        }
        private async Task<List<RuleDocumentRsp>> RetrieveDocumentsFor(Guid id, string entityFolder)
        {
            var relativeUrl = _documentLocationAppService.GetDocLocationByRegardingId(id);
            return await RetrieveDocumentsList($"{entityFolder}/{relativeUrl}");
        }
        private void SetMasterName(List<RuleDocumentRsp> documents, string name, string nameAr)
        {
            foreach (var doc in documents)
            {
                doc.MasterRecordName = name;
                doc.MasterRecordNameAr = nameAr;
            }
        }
        private async Task<List<RuleDocumentRsp>> RetrieveDocumentsList(string targetURL)
        {
            List<RuleDocumentRsp> response = new List<RuleDocumentRsp>();

            if (!string.IsNullOrEmpty(targetURL))
            {
                response = _fileService.GetDocumentsByTargetUrl(targetURL).Select(x => new RuleDocumentRsp
                {
                    DocumentCreatedOnInUTC = x.DocumentCreatedOnInUTC,
                    DocumentId = x.DocumentId,
                    DocumentName = x.DocumentName,
                    DocumentType = x.DocumentType,
                    DocumentSizeInBytes = x.DocumentSizeInBytes,
                    DocumentPath = targetURL + "/" + x.DocumentName
                }).ToList();
            }
            return response;
        }

    }
}
