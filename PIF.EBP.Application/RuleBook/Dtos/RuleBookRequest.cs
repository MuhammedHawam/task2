using Microsoft.Xrm.Sdk;
using PIF.EBP.Application.MetaData.DTOs;
using PIF.EBP.Application.Shared.AppRequest;
using System;
using System.Collections.Generic;

namespace PIF.EBP.Application.RuleBook.DTOs
{
    public class RuleBookRequest
    {
        public PagingRequest PagingRequest { get; set; }
        public string Keyword { get; set; }
        public EntityReferenceDto Chapter { get; set; }
        public OptionSetValue PCLifeCycleStage { get; set; }
        public OptionSetValue TypeOfRequirment { get; set; }
        public DateTime LastUpdate { get; set; }
        public OptionSetValue Theme { get; set; }

    }

    public class RulesReponse
    {
        public List<RuleDto> RuleList { get; set; }
        public RuleLocationDto Location { get; set; }  
        public int TotalCount { get; set; }
    }

    public class RuleLocationDto
    {
        public string SubSectionName { get; set; }
        public string SubSectionNameAr { get; set; }
        public string SectionName { get; set; }
        public string SectionNameAr { get; set; }
        public string ChapterName { get; set; }
        public string ChapterNameAr { get; set; }

    }

    public class RuleDto
    {
        public string Title { get; set; }
        public string TitleAr { get; set; }
        public string Id { get; set; }
        public string Number { get; set; }
    }
    public class SubRulesResponse
    {
        public List<SubRuleDto> SubRuleList { get; set; }
        public RuleDetailsDto Rule { get; set; }
    }
    public class RuleDetailsDto
    {
        public DateTime? ModifiedOn { get; set; }
        public string Description { get; set; }
        public string DescriptionAr { get; set; }
        public string Theme { get; set; }
        public string ThemeAr { get; set; }
        public string Name { get; set; }
        public string NameAr { get; set; }
        public string TypeOfRequirementLabel { get; set; }
        public string TypeOfRequirementLabelAr { get; set; }
        public string PcLifeCycleStageLabel { get; set; }
        public string PcLifeCycleStageLabelAr { get; set; }
        public string SubSectionName { get; set; }
        public string SubSectionNameAr { get; set; }
        public Guid SubSectionId { get; set; }
        public string SectionName { get; set; }
        public string SectionNameAr { get; set; }
        public string ChapterName { get; set; }
        public string ChapterNameAr { get; set; }
    }
    public class SubRuleDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string NameAr { get; set; }

        public string Description { get; set; }
        public string DescriptionAr { get; set; }

    }

    public class RuleSearchRequest
    {
        public string RuleBookId { get; set; }
        public string Locale { get; set; }
        public string Keyword { get; set; }
        public Guid? ChapterId { get; set; }
        public int? Stage { get; set; }
        public int? TypeOfRequirement { get; set; }
        public DateTime? LastUpdatedFrom { get; set; }
        public DateTime? LastUpdatedTo { get; set; }

        public int? Theme { get; set; }
        public int PageNumber { get; set; } = 1; 
        public int PageSize { get; set; } = 10; 
    }
    public class PagedRuleSearchResult
    {
        public List<RuleSearchResultDto> Items { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; } 
        public int TotalCount { get; set; }
    }
    public class RuleSearchResultDto
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string StageLabel { get; set; }
        public string TypeOfRequirementLabel { get; set; }
        public string Theme { get; set; }
        public string ChapterName { get; set; }
    }
    public class RulesRequest
    {
        public Guid SubSectionId { get; set; }
        public PagingRequest PagingRequest { get; set; }
        public string SearchKeyword { get; set; }
    }
}
