namespace PIF.EBP.Core.FileManagement.DTOs
{
    public class AttendeeEventDetailsDto
    {
        public string Id { get; set; }
        public string RSVP { get; set; }
        public string Table { get; set; }
        public string Seat { get; set; }
        public string RefId { get; set; }
        public string CheckedIn { get; set; }
        public string GuestName { get; set; }
        public string GuestRole { get; set; }
        public FieldLookupValueDto Attendee { get; set; }
        public FieldLookupValueDto Event { get; set; }
    }
    public class FieldLookupValueDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
    public class EventDetailsDto : FieldLookupValueDto
    {         
         public string DressCode { get; set; }
         public string EventTime { get; set; }
         public string EventDate { get; set; }
         public string Location { get; set; }
         public string ContactNumber { get; set; }
         public bool? Hide { get; set; }
         public bool? ShowSeatingInfo { get; set; }
     }
}
