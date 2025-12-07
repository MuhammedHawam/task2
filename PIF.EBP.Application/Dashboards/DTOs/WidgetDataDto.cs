using PIF.EBP.Application.MetaData.DTOs;
using PIF.EBP.Application.PortalAdministration.DTOs;
using System;
using System.Collections.Generic;

namespace PIF.EBP.Application.Dashboards.DTOs
{
    public class WidgetDataDto
    {
        public bool ShowTutorial { get; set; } = false;
        public RequestSummary RequestSummary { get; set; }
        public List<UserTask> Tasks { get; set; } = new List<UserTask>();
        public List<CalendarDetailsDto> CalendarDetails { get; set; } = new List<CalendarDetailsDto>();
        public ContactDetails ContactDetails { get; set; }
        public CompanyDto CompanyDetails { get; set; }
        public List<UserProfileQuickAction> QuickActions { get; set; } = new List<UserProfileQuickAction>();
    }

    public class OpenRequests
    {
        public int DueMonth { get; set; } = 0;
        public int DueWeek { get; set; } = 0;
        public int OverDue { get; set; } = 0;
    }

    public class SubmittedPendingRequests
    {
        public int UnderReview { get; set; } = 0;
        public int Approved { get; set; } = 0;
        public int Returned { get; set; } = 0;
    }

    public class UserTask
    {
        public Guid Id { get; set; }
        public Guid ParentRequestId { get; set; }
        public string Name { get; set; }
        public string NameAr { get; set; }
        public string Description { get; set; }
        public string DescriptionAr { get; set; }
        public EntityOptionSetDto Status { get; set; }
    }
    public class RequestSummary
    {
        public int TotalNewRequests { get; set; } = 0;
        public int TotalOpenRequests { get; set; } = 0;
        public int TotalSubmittedPendingRequests { get; set; } = 0;
        public OpenRequests OpenRequestsDetails { get; set; }
        public SubmittedPendingRequests SubmittedPendingRequestsDetails { get; set; }
        public NewRequests NewRequestsDetails { get; set; }
    }

    public class ContactDetails
    {
        public byte[] ContactImage { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FirstNameAr { get; set; }
        public string LastNameAr { get; set; }
    }

    public class UserProfileQuickAction
    {
        public string Name { get; set; }
        public string NameAr { get; set; }
        public string RequestStepNumber { get; set; }
        public Guid Id { get; set; }
        public Guid ParentRequestId { get; set; }
        public string ParentRequestName { get; set; }
        public string ParentRequestNameAr { get; set; }
        public string ParentRequestNumber { get; set; }
        public string Status { get; set; }
    }

    public class NewRequests
    {
        public int FirstMonth { get; set; } = 0;
        public int SecondMonth { get; set; } = 0;
        public int ThirdMonth { get; set; } = 0;
    }
}
