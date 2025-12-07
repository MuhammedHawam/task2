using PIF.EBP.Application.Commercialization.DTOs.IESMServiceModels;
using System.Collections.Generic;

namespace PIF.EBP.Application.Commercialization.DTOs
{
    public class ServiceFileResponseDto
    {
        public string RequestName { get; set; }
        public string RequestNumber { get; set; }
        public string ReturnedReason { get; set; }
        public string CompletionComment { get; set; }
        public string RejectionComment { get; set; }
        public EsmOptionsDto State { get; set; }
        public List<RequestAttachmentDto> Attachments { get; set; } = new List<RequestAttachmentDto>();
        public List<SurveyQuestionDto> SurveyQuestions { get; set; } = new List<SurveyQuestionDto>();

    }
    public class RequestAttachmentDto
    {
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public string SysId { get; set; }
        public string Size { get; set; }
        public string IssueDate { get; set; }
    }
}
