using Microsoft.Xrm.Sdk;
using PIF.EBP.Application.AccessManagement.DTOs;
using PIF.EBP.Application.MetaData.DTOs;
using PIF.EBP.Application.Notification.DTOs;
using PIF.EBP.Application.Shared.AppResponse;
using System;
using System.Collections.Generic;

namespace PIF.EBP.Application.RuleBook.DTOs
{
    public class SearchDropdownReponse
    {
        public List<EntityOptionSetDto> PCLifeCycleStage { get; set; }
        public List<EntityOptionSetDto> TypeOfRequirment { get; set; }
        public List<EntityOptionSetDto> Theme { get; set; }
    }
    public class RuleDocumentRsp
    {
        public string DocumentName { get; set; }
        public string DocumentId { get; set; }
        public string DocumentPath { get; set; }
        public string DocumentType { get; set; }
        public DateTime DocumentCreatedOnInUTC { get; set; }
        public long DocumentSizeInBytes { get; set; }
        public string MasterRecordName { get; set; }
        public string MasterRecordNameAr { get; set; }
    }
    public class RuleWithSubRules
    {
        public Guid RuleId { get; set; }
        public string RuleName { get; set; }
        public string RuleNameAr { get; set; }
        public List<SubRuleInfo> SubRules { get; set; } = new List<SubRuleInfo>();
    }

    // Strongly typed sub-rule
    public class SubRuleInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string NameAr { get; set; }
    }
}
