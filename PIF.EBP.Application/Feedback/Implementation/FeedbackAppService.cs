using Microsoft.Xrm.Sdk;
using PIF.EBP.Application.Feedback.DTOs;
using PIF.EBP.Application.FileScanning;
using PIF.EBP.Application.PortalConfiguration;
using PIF.EBP.Application.Shared;
using PIF.EBP.Core.CRM;
using PIF.EBP.Core.Exceptions;
using PIF.EBP.Core.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static PIF.EBP.Application.Shared.Enums;

namespace PIF.EBP.Application.Feedback.Implementation
{
    public class FeedbackAppService : IFeedbackAppService
    {
        private readonly ICrmService _crmService;
        private readonly ISessionService _sessionService;
        private readonly IPortalConfigAppService _portalConfigAppService;
        private readonly IFileScanningService _fileScanService;
        private readonly IFeedbackFileUploaderService _feedbackFileUploaderService;

        public FeedbackAppService(ICrmService crmService, ISessionService sessionService,
            IPortalConfigAppService portalConfigAppService, IFileScanningService fileScanService, IFeedbackFileUploaderService feedbackFileUploaderService)
        {
            _crmService = crmService;
            _sessionService = sessionService;
            _portalConfigAppService = portalConfigAppService;
            _fileScanService = fileScanService;
            _feedbackFileUploaderService=feedbackFileUploaderService;
        }

        public async Task<string> CreateFeedback(FeedbackDto feedbackDto)
        {
            var Entity = new Entity(EntityNames.ShareFeedback);

            Entity["regardingobjectid"] = new EntityReference(EntityNames.Account, new Guid(_sessionService.GetCompanyId()));
            Entity["pwc_company"] = new EntityReference(EntityNames.Account, new Guid(_sessionService.GetCompanyId()));
            Entity["pwc_contact"] = new EntityReference(EntityNames.Contact, new Guid(_sessionService.GetContactId()));
            Entity["source"] = new OptionSetValue(1);//Portal
            var supportTeam = (_portalConfigAppService
                .RetrievePortalConfiguration(new List<string> { PortalConfigurations.PIFSupportTeam })).FirstOrDefault();
            Entity["ownerid"] = new EntityReference(EntityNames.Team, new Guid(supportTeam.Value));
            if (feedbackDto.TypeId == (int)ShareFeedbackType.ReportBug)
            {
                Entity["title"] = "Report Bug";
                Entity["comments"] = feedbackDto.Description;
                Entity["pwc_feedbacktypecode"] = new OptionSetValue(feedbackDto.TypeId);
            }
            else if (feedbackDto.TypeId == (int)ShareFeedbackType.ShareFeedback)
            {
                
                Entity["title"] = "Share Feedback";
                Entity["comments"] = feedbackDto.Description;
                Entity["pwc_feedbacktypecode"] = new OptionSetValue(feedbackDto.TypeId);
                

                if (feedbackDto.TypeofFeedbackId.HasValue && (feedbackDto.TypeofFeedbackId.Value == (int)FeedbackType.suggestion || feedbackDto.TypeofFeedbackId.Value == (int)FeedbackType.Complaint))
                {
                    Entity["pwc_portaltypeoffeedback"] = new OptionSetValue(feedbackDto.TypeofFeedbackId.Value);
                }
                else
                {
                    throw new UserFriendlyException("InvalidTypeofFeedbackValue", System.Net.HttpStatusCode.BadRequest);
                }
            }
            else
            {
                throw new UserFriendlyException("InvalidFeedbackTypeValue", System.Net.HttpStatusCode.BadRequest);
            }

            var Id = _crmService.Create(Entity, EntityNames.ShareFeedback);

            if (Id != Guid.Empty && feedbackDto.AttachmentAttributes != null && !string.IsNullOrEmpty(feedbackDto.AttachmentAttributes.FileName))
            {
                EntityReference entityReference = new EntityReference()
                {
                    Id = Id,
                    LogicalName = EntityNames.ShareFeedback
                };
                var configurations = _portalConfigAppService.RetrievePortalConfiguration(new List<string> { PortalConfigurations.EnableFileScanning });
                bool.TryParse(configurations.SingleOrDefault(a => a.Key == PortalConfigurations.EnableFileScanning).Value, out bool enableFileScanning);
                if (enableFileScanning)
                {
                    string companyId = _sessionService.GetCompanyId();
                    var metaData = new
                    {
                        CompanyId = companyId,
                        FeedbackId=Id.ToString(),
                        FeedbackFileExtension=feedbackDto.AttachmentAttributes.FileExtension
                    };
                    var response = await _fileScanService.AnalyzeFile(Convert.FromBase64String(feedbackDto.AttachmentAttributes.FileContent), feedbackDto.AttachmentAttributes.FileName, metaData);

                    if (Guid.TryParse(response, out Guid dataId))
                    {
                        return response;
                    }

                }
                else
                {
                    _feedbackFileUploaderService.AttachFileToCRMRecord(entityReference, "pwc_attachment1", feedbackDto.AttachmentAttributes);
                }
                
            }

            return "Feedback record has been created";
        }

        
    }
}
