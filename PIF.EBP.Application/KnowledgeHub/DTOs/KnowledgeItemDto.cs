using System.Collections.Generic;

namespace PIF.EBP.Application.KnowledgeHub.DTOs
{
    public class KnowledgeItemDto
    {
        public AnnouncementDto Announcements { get; set; }
        public List<FAQDto> FAQs { get; set; }
        public ArticleDto Articles { get; set; }
        public TemplateDto Templates { get; set; }
        public PlaybookDto Playbooks { get; set; }
        public UserManualDto UserManuals { get; set; }
    }
}
