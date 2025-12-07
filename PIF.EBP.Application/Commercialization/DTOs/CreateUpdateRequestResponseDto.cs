namespace PIF.EBP.Application.Commercialization.DTOs
{
    public class CreateUpdateRequestResponseDto
    {
        public string RequestId { get; set; }
        public string Number { get; set; }
        public bool SentForScanning { get; set; } = false;
    }
}
