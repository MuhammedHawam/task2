using PIF.EBP.Application.AccessManagement.DTOs;
using PIF.EBP.Application.MetaData.DTOs;
using PIF.EBP.Application.Notification.DTOs;
using PIF.EBP.Application.Shared.AppRequest;
using PIF.EBP.Application.Shared.AppResponse;
using System;
using System.Collections.Generic;

namespace PIF.EBP.Application.RuleBook.DTOs
{

    public class SectionRequest
    {
        public PagingRequest PagingRequest { get; set; }
        public Guid chapterId { get; set; }

    }
    public class SectionResponse
    {
        public List<SectionDto> Sections { get; set; }
        public int TotalCount { get; set; }

    }

    public class SectionDto
    {
        public string Name { get; set; }
        public string NameAr { get; set; }
        public string Number { get; set; }
        public List<SubSectionDto> SubSections { get; set; }
    }

    public class SubSectionDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string NameAr { get; set; }
        public string Description { get; set; }
        public string DescriptionAr { get; set; }
        public string Number { get; set; }
        public DateTime LastUpdatedOn { get; set; }

    }


}
