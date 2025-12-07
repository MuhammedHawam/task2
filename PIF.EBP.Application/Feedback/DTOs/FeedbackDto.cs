namespace PIF.EBP.Application.Feedback.DTOs
{
    public class FeedbackDto
    {
        public int TypeId { get; set; }
        public int? TypeofFeedbackId { get; set; }
        public string Description { get; set; }
        public AttachmentAttributesDto AttachmentAttributes { get; set; }
    }

    public class AttachmentAttributesDto
    {
        public string FileName { get; set; }
        public string FileExtension { get; set; }
        public string FileContent { get; set; }
    }
}
