using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIF.EBP.Core.Community.DTOs
{
    public class EmailNotificationModel
    {
        public List<string> to { get; set; }
        public List<string> cc { get; set; }
        public List<string> bcc { get; set; }
        public int templateType { get; set; }
        public IReadOnlyDictionary<string, string> templateData { get; set; }
        public string subject { get; set; }
        public string body { get; set; }
        public bool isHtml { get; set; }
        public List<EmailAttachment> attachments { get; set; }
        public string correlationId { get; set; }
        public string sourceService { get; set; }
    }

    public class EmailAttachment
    {
        public string fileName { get; set; }
        public string contentType { get; set; }
        public string content { get; set; }
    }
}
