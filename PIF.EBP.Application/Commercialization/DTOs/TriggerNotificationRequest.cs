namespace PIF.EBP.Application.Commercialization.DTOs
{
    public class TriggerNotificationRequest
    {
        public string RequestId { get; set; }
        public string RequestNumber { get; set; }
        public string Status { get; set; }
        public string ContactId { get; set; }
        public string CompanyId { get; set; }
        public string Comment { get; set; }
    }

    public class TriggerNotificationResponse
    {
        public string Status { get; set; }
        public string Message { get; set; }
    }
}
