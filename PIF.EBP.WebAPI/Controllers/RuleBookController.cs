using PIF.EBP.Application.RuleBook;
using PIF.EBP.Application.RuleBook.DTOs;
using PIF.EBP.Application.Sharepoint;
using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.WebAPI.Middleware.ActionFilter;
using System;
using System.Data;
using System.Threading.Tasks;
using System.Web.Http;

namespace PIF.EBP.WebAPI.Controllers
{
    [ApiResponseWrapper]
    [RoutePrefix("RuleBook")]
    public class RuleBookController : BaseController
    {
        private readonly IRuleBookAppService _ruleBookAppService;

        public RuleBookController()
        {
            _ruleBookAppService = WindsorContainerProvider.Container.Resolve<IRuleBookAppService>();
        }
        [HttpPost]
        [Route("get-rules-by-search")]
        public async Task<IHttpActionResult> SearchRules(RuleSearchRequest request)
        {
            var result = await _ruleBookAppService.SearchRulesAsync(request);
            return Ok(result);
        }
        [HttpGet]
        [Route("get-rulebook")]
        public async Task<IHttpActionResult> RetrieveRuleBook()
        {
            var result = await _ruleBookAppService.RetrieveRuleBookList();

            return Ok(result);
        }
        [HttpPost]
        [Route("get-sections-by-chapter")]
        public async Task<IHttpActionResult> RetrieveSectionsByChapter(SectionRequest oSectionRequest)
        {
            var result = await _ruleBookAppService.RetrieveSectionsByChapter(oSectionRequest);

            return Ok(result);
        }
        [HttpGet]
        [Route("get-dropdowns")]
        public async Task<IHttpActionResult> RetrieveDropDownLists()
        {
            var result = await _ruleBookAppService.RetrieveDropDownLists();

            return Ok(result);
        }
        [HttpPost]
        [Route("get-rules-by-subsection")]
        public async Task<IHttpActionResult> RetrieveRulesBySubSection([FromBody] RulesRequest oRuleRequest)
        {
            var result = await _ruleBookAppService.RetrieveRulesBySubSection(oRuleRequest);
            return Ok(result);
        }
        [HttpGet]
        [Route("get-subrules-by-rule/{ruleId:guid}")]
        public async Task<IHttpActionResult> GetSubRulesByRule(Guid ruleId)
        {
            var result = await _ruleBookAppService.RetrieveSubRulesWithRuleDetails(ruleId);
            return Ok(result);
        }
        [HttpGet]
        [Route("get-all-rule-documents/{ruleId:guid}")]
        public async Task<IHttpActionResult> GetRulesDocuments(Guid ruleId)
        {
            var result = await _ruleBookAppService.GetRuleAndSubRuleDocuments(ruleId);
            return Ok(result);
        }
    }
}