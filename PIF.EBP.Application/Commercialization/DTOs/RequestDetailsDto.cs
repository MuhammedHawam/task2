using PIF.EBP.Application.Commercialization.DTOs.IESMServiceModels;
using PIF.EBP.Application.MetaData.DTOs;
using System;
using System.Collections.Generic;

namespace PIF.EBP.Application.Commercialization.DTOs
{
    public class RequestDetailsDto
    {


        public string RequestTitle { get; set; }
        public string RequestTitleAr { get; set; }
        public string ServiceDescription { get; set; }
        public string ServiceDescriptionAr { get; set; }
        public string RequestNumber { get; set; }
        public string ReferenceNumber { get; set; }
        public string ReturnedReason { get; set; }
        public string CompletionComment { get; set; }
        public string RejectionComment { get; set; }
        public EntityReferenceDto RequestedBy { get; set; }
        public DateTime? InitiationDate { get; set; }
        public EsmOptionsDto State { get; set; }
        public EntityReferenceDto Company { get; set; }
        public DateTime? DueDate { get; set; }
        public string SurveyStatus { get; set; }
        public List<FormPageDto> FormPages { get; set; }

    }
}
