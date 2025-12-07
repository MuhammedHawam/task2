using PIF.EBP.Application.MetaData.DTOs;
using System;

namespace PIF.EBP.Application.Notification.DTOs
{
    public class NotificationDto
    {
        public Guid Id { get; set; }
        public string RoleAssociationId { get; set; }
        public string Name { get; set; }
        public string NameAr { get; set; }
        public string Description { get; set; }
        public string DescriptionAr { get; set; }
        public DateTime CreatedOn { get; set; }
        public EntityOptionSetDto ReadStatus { get; set; }
        public EntityOptionSetDto Type { get; set; }
        public AppointmentDto Appointment { get; set; }
        public EntityReferenceDto Contact { get; set; }
        public EntityReferenceDto Company { get; set; }
        public EntityReferenceDto RelatesTo { get; set; }
        public EntityReferenceDto Organizedby { get; set; }
        public EntityReferenceDto RequestStep { get; set; }
        public EntityReferenceDto Request { get; set; }
        public EntityReferenceDto PortalRole { get; set; }
        public string DueDate { get; set; }
        public bool OverdueFlag { get; set; }
        public DateTime EventDate { get; set; }
    }
    public class AppointmentDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string TitleAr { get; set; }
        public int? Type { get; set; }
        public DateTime? Date { get; set; }
    }
}
