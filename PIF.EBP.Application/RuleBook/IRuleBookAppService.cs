using PIF.EBP.Application.KnowledgeHub.DTOs;
using PIF.EBP.Application.RuleBook.DTOs;
using PIF.EBP.Core.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PIF.EBP.Application.RuleBook
{
    public interface IRuleBookAppService : ITransientDependency
    {
        Task<RuleBookResponse> RetrieveRuleBookList();
        Task<SectionResponse> RetrieveSectionsByChapter(SectionRequest oSectionRequest);
        Task<PagedRuleSearchResult> SearchRulesAsync(RuleSearchRequest request);
        Task<SearchDropdownReponse> RetrieveDropDownLists();
        Task<RulesReponse> RetrieveRulesBySubSection(RulesRequest oRuleRequest);
        Task<SubRulesResponse> RetrieveSubRulesWithRuleDetails(Guid ruleId);
        Task<List<RuleDocumentRsp>> GetRuleAndSubRuleDocuments(Guid ruleId);
    }
}
