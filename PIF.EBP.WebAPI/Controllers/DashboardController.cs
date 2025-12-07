using PIF.EBP.Application.Dashboards;
using PIF.EBP.Application.Dashboards.DTOs;
using PIF.EBP.Application.PortalAdministration;
using PIF.EBP.Application.Settings;
using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.Core.Exceptions;
using PIF.EBP.Core.Session;
using PIF.EBP.WebAPI.Middleware.ActionFilter;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using static PIF.EBP.Application.Shared.Enums;

namespace PIF.EBP.WebAPI.Controllers
{
    [ApiResponseWrapper]
    [RoutePrefix("Dashboard")]
    public class DashboardController : BaseController
    {
        private readonly IDashboardAppService _dashboardAppService;
        private readonly IUserProfileAppService _userProfileAppService;
        private readonly IPortalAdministrationAppService _portalAdministrationAppService;
        private readonly ISessionService _sessionService;

        public DashboardController()
        {
            _dashboardAppService = WindsorContainerProvider.Container.Resolve<IDashboardAppService>();
            _userProfileAppService = WindsorContainerProvider.Container.Resolve<IUserProfileAppService>();
            _portalAdministrationAppService = WindsorContainerProvider.Container.Resolve<IPortalAdministrationAppService>();
            _sessionService = WindsorContainerProvider.Container.Resolve<ISessionService>();
        }

        [HttpGet]
        [Route("get-widget-data")]
        public async Task<IHttpActionResult> GetWidgetData(int? scope = (int)ScopeWidgetData.All, DateTime? CalendarDate = null, int? ScheduleFilter = (int)ScopeSchedule.All, Guid? TaskFilter = null)
        {
            var result = await _dashboardAppService.RetrieveWidgetData(scope, CalendarDate, ScheduleFilter, TaskFilter);

            result.ContactDetails = _userProfileAppService.RetrieveUserDetails();
            result.CompanyDetails = (await _portalAdministrationAppService.RetrievecompaniesByContactId()).FirstOrDefault(x => x.Id == _sessionService.GetCompanyId());

            return Ok(result);
        }

        [HttpPost]
        [Route("get-calendar-by-Month")]
        public async Task<IHttpActionResult> GetCalendarDetailsByMonth(CalendarRequestDtoRephrased calendarRequestDto)
        {

            var calendarRequest = new CalendarRequestDto
            {
                CalendarDate = calendarRequestDto.CalendarDate,
                ScheduleFilter = calendarRequestDto.ScheduleFilter,
                SearchFilter = calendarRequestDto.SearchFilter,
                OptionalId = calendarRequestDto.OptionalId,
                TypeOfId = calendarRequestDto.TypeOfId,
            };
            var result = await _dashboardAppService.RetrieveCalendarDetailsByMonth(calendarRequest);
            result.CompanyDetails = (await _portalAdministrationAppService.RetrievecompaniesByContactId()).FirstOrDefault(x => x.Id == _sessionService.GetCompanyId());

            return Ok(result);
        }

        [HttpGet]
        [Route("site-search")]
        public async Task<IHttpActionResult> GetSiteSearchResult(string searchText, int? scope = (int)ScopeWidgetData.All)
        {
            if (string.IsNullOrEmpty(searchText))
            {
                throw new UserFriendlyException("RequiredParametersSearch", System.Net.HttpStatusCode.BadRequest);
            }

            var result = await _dashboardAppService.RetrieveSiteSearchResult(searchText.Trim(), scope);

            return Ok(result);
        }
    }
}
