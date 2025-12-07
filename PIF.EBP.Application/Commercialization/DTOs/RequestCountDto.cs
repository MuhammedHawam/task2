namespace PIF.EBP.Application.Commercialization.DTOs
{
    public class RequestCountDto
    {
        public double UnderReview { get; set; }
        public double Returned { get; set; }
        public double Rejected { get; set; }
        public double TotalPending { get; set; }
        public double Completed { get; set; }
    }
}
