using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PIF.EBP.Application.SMTPNotificaation.DTOs
{
    public class SendEmailDto
    {
        [Required]
        public string Subject { get; set; }

        [Required]
        public string Body { get; set; }

        [Required]
        [MinLength(1)]
        public string[] ToEmails { get; set; }

        public string[] CcEmails { get; set; }

        public string[] BccEmails { get; set; }

        public List<EmailAttachmentDto> Attachments { get; set; }

        public bool IsHtmlBody { get; set; }
    }

    public class EmailAttachmentDto
    {
        [Required]
        public string FileName { get; set; }

        [Required]
        public byte[] FileContent { get; set; }

        public string MimeType { get; set; }
    }
}
