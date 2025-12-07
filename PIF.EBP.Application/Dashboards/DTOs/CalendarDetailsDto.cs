using PIF.EBP.Application.AccessManagement.DTOs;
using PIF.EBP.Application.Dashboards.Implementation;
using PIF.EBP.Application.MetaData.DTOs;
using PIF.EBP.Application.PortalAdministration.DTOs;
using PIF.EBP.Application.Shared.AppRequest;
using PIF.EBP.Application.Shared.AppResponse;
using System;
using System.Collections.Generic;
using static PIF.EBP.Application.Shared.Enums;

namespace PIF.EBP.Application.Dashboards.DTOs
{
    public class CalendarDetailsDto
    {
        public Guid Id { get; set; }
        public string EventType { get; set; }
        public string EventTypeAr { get; set; }
        public string Title { get; set; }
        public string TitleAr { get; set; }
        public string Description { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string OrganizerName { get; set; }
        public string OrganizerNameAr { get; set; }
        public Guid OrganizerId { get; set; }
        public string Location { get; set; }
        public List<Attendees> Attendees { get; set; }
    }

    public class Attendees
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string NameAr { get; set; }
        public bool IsRequired { get; set; }
    }

    public class CalendarDto
    {
        public ListPagingResponse<CalendarDetailsDto> Meetings { get; set; } = new ListPagingResponse<CalendarDetailsDto>();
        public ListPagingResponse<CalendarDetailsDto> Events { get; set; } = new ListPagingResponse<CalendarDetailsDto>();
        public List<ProfileTasks> Tasks { get; set; } = new List<ProfileTasks>();
        public ListPagingResponse<CalendarTasks> Requests { get; set; } = new ListPagingResponse<CalendarTasks>();
        public CompanyDto CompanyDetails { get; set; }
    }

    public class SiteSearchDto 
    {
        public List<CalendarDetailsDto> Meetings { get; set; } = new List<CalendarDetailsDto>();
        public List<CalendarDetailsDto> Events { get; set; } = new List<CalendarDetailsDto>();
        public List<CalendarTasks> Tasks { get; set; } = new List<CalendarTasks>();
        public List<CalendarTasks> Requests { get; set; } = new List<CalendarTasks>();
    }

    public class CalendarTasks
    {
        public Guid? RequestStepId { get; set; }
        public DateTime? DueDate { get; set; }
        public bool OverDue { get; set; }
        public EntityReferenceDto Company { get; set; }
        public string Initiator { get; set; }
        public Guid InitiatorId { get; set; }
        public string Name { get; set; }
        public string NameAr { get; set; }
        public string Description { get; set; }
        public string DescriptionAr { get; set; }
        public EntityReferenceDto Request { get; set; }
        public EntityOptionSetDto Status { get; set; }
        public string Type { get; set; }
        public string TypeAr { get; set; }
        public PortalRole PortalRole { get; set; }
        public string PortalContactId { get; set; }
    }

    public class CalendarRequestDto
    {
        public DateTime CalendarDate { get; set; }
        public int? ScheduleFilter { get; set; } = (int)ScopeSchedule.All;
        public string SearchFilter { get; set; } = null;
        public Guid? OptionalId { get; set; } = null;
        public int? TypeOfId { get; set; } = (int)ScopeSearchCalender.Requests;
        public Guid? CompanyId { get; set; } = null;
        public PagingRequest PagingRequest { get; set; }
    }

    public class CalendarRequestDtoRephrased 
    {
        public DateTime CalendarDate { get; set; }
        public int? ScheduleFilter { get; set; } = (int)ScopeSchedule.All;
        public string SearchFilter { get; set; } = null;
        public Guid? OptionalId { get; set; } = null;
        public int? TypeOfId { get; set; } = (int)ScopeSearchCalender.Requests;
    }
}
