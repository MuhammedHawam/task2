using PIF.EBP.Application.AccessManagement.DTOs;
using PIF.EBP.Application.MetaData.DTOs;
using PIF.EBP.Application.Notification.DTOs;
using PIF.EBP.Application.Shared.AppResponse;
using System;
using System.Collections.Generic;

namespace PIF.EBP.Application.RuleBook.DTOs
{
    public class RuleBookResponse
    {
        public List<RuleBookDto> RuleBookList { get; set; }
    }
    public class RuleBookDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string NameAr { get; set; }
        public string Description { get; set; }
        public List<ChapterDto> Chapters { get; set; } = new List<ChapterDto>();
    }
    public class ChapterDto

    {
        public String Id { get; set; }
        public string Title { get; set; }
        public string TitleAr { get; set; }
        public string Number { get; set; }

    }

}
