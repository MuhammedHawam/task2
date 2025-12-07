using PIF.EBP.Application.Dashboards.DTOs;
using PIF.EBP.Core.DependencyInjection;
using System;
using System.Threading.Tasks;
using static PIF.EBP.Application.Shared.Enums;

namespace PIF.EBP.Application.Dashboards
{
    public interface IDashboardAppService : ITransientDependency
    {
        Task<WidgetDataDto> RetrieveWidgetData(int? scope = (int)ScopeWidgetData.All, DateTime? CalendarDate = null, int? ScheduleFilter = (int)ScopeSchedule.All, Guid? TaskFilter = null);
        Task<CalendarDto> RetrieveCalendarDetailsByMonth(CalendarRequestDto calendarRequestDto);
        Task<SiteSearchDto> RetrieveSiteSearchResult(string searchText, int? scope = (int)ScopeWidgetData.All);
    }
}
