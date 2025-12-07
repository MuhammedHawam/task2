using PIF.EBP.Application.MetaData.DTOs;
using System;
using System.Collections.Generic;

namespace PIF.EBP.Application.KnowledgeHub.DTOs
{
    public class AnnouncementDto
    {
        public int ItemCount { get; set; }
        public int UnReadCount { get; set; }
        public List<Announcement> Announcements { get; set; }
    }

    public class Announcement
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string TitleAr { get; set; }
        public string Description { get; set; }
        public string DescriptionAr { get; set; }
        public DateTime Created { get; set; }
        public bool IsUnRead { get; set; }
        public bool IsPin { get; set; }
        public EntityOptionSetDto Category { get; set; }
        public EntityReferenceDto ContentClassification { get; set; }
        public string ShortDescription { get; set; }
        public string ShortDescriptionAr { get; set; }
        
    }
}
