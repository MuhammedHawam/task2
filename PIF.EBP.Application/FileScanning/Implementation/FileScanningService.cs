using System;
using System.Threading.Tasks;
using PIF.EBP.Core.FileScanning;
using PIF.EBP.Application.Sharepoint;
using PIF.EBP.Core.FileManagement.DTOs;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk;
using System.Linq;
using PIF.EBP.Core.CRM;
using PIF.EBP.Application.Shared;
using PIF.EBP.Application.Shared.Helpers;
using PIF.EBP.Application.MetaData.DTOs;
using PIF.EBP.Application.DataCollection;
using PIF.EBP.Application.KnowledgeHub;
using PIF.EBP.Application.Feedback;
using PIF.EBP.Application.Feedback.DTOs;
using PIF.EBP.Application.Commercialization;
using PIF.EBP.Core.ESM.DTOs;
using PIF.EBP.Application.InfraBase;

namespace PIF.EBP.Application.FileScanning.Implementation
{
    public class FileScanningService : IFileScanningService
    {
        private readonly IExternalFileScanningService _externalFileScanningService;
        private readonly IHexaDocumentManagementService _sharepointService;
        private readonly IDataCollectionService _dataCollectionService;
        private readonly ICrmService _crmService;
        private readonly IKnowledgeHubFileUploaderService _knowledgeHubFileUploaderService;
        private readonly IFeedbackFileUploaderService _feedbackFileUploaderService;
        private readonly ICommercializationAppService _commercializationAppService;
        private readonly IInfraBaseFileUploaderService _infraBaseFileUploaderService;

        public FileScanningService(IExternalFileScanningService externalFileScanningService, IHexaDocumentManagementService sharepointService,
                                   IDataCollectionService dataCollectionService, ICrmService crmService, 
                                   IKnowledgeHubFileUploaderService knowledgeHubFileUploaderService, IFeedbackFileUploaderService feedbackFileUploaderService,
                                   ICommercializationAppService commercializationAppService, IInfraBaseFileUploaderService infraBaseFileUploaderService)
        {
            _externalFileScanningService = externalFileScanningService;
            _sharepointService = sharepointService;
            _dataCollectionService = dataCollectionService;
            _crmService = crmService;
            _knowledgeHubFileUploaderService = knowledgeHubFileUploaderService;
            _feedbackFileUploaderService = feedbackFileUploaderService;
            _commercializationAppService = commercializationAppService;
            _infraBaseFileUploaderService = infraBaseFileUploaderService;
        }

        public async Task<string> AnalyzeFile(byte[] fileContent, string fileName, object metaData)
        {
            var result = await _externalFileScanningService.AnalyzeFile(fileContent, fileName, metaData);
            return result;
        }

        public async Task GetFileResult(string content, string dataId)
        {
            UploadDocumentsDto uploadDocumentsDto = await _externalFileScanningService.GetFileResult(content, dataId);

            if (uploadDocumentsDto.Documents !=null && uploadDocumentsDto.KnowledgeItemId !=Guid.Empty)
            {
                await _knowledgeHubFileUploaderService.KnowledgeItemUpload(uploadDocumentsDto);//KnowledgeItem
                return;
            }
            else if (uploadDocumentsDto.Documents !=null && uploadDocumentsDto.FeedbackId !=Guid.Empty)
            {
                EntityReference entityReference = new EntityReference()
                {
                    Id = uploadDocumentsDto.FeedbackId,
                    LogicalName = EntityNames.ShareFeedback
                };
                if (uploadDocumentsDto.Documents.Any())
                {
                    var document = uploadDocumentsDto.Documents.FirstOrDefault();
                    AttachmentAttributesDto attachmentAttributes = new AttachmentAttributesDto
                    {
                        FileContent = document.DocumentContent,
                        FileName = document.DocumentName,
                        FileExtension = uploadDocumentsDto.FeedbackFileExtension
                    };
                    _feedbackFileUploaderService.AttachFileToCRMRecord(entityReference, "pwc_attachment1", attachmentAttributes);//Feedback
                }
                return;
            }
            else if (uploadDocumentsDto.Documents != null && uploadDocumentsDto.Documents.Count > 0 && uploadDocumentsDto.RegardingObjectId != Guid.Empty) //HexaRequest
            {
                await _sharepointService.UploadAttachmentsAsync(uploadDocumentsDto);
                FillRequestDocumentComments(uploadDocumentsDto.RegardingObjectId, uploadDocumentsDto.FileScanningResult, dataId);
                return;

            }
            else if (uploadDocumentsDto.Documents != null && uploadDocumentsDto.Documents.Count > 0 && !string.IsNullOrEmpty(uploadDocumentsDto.ReferenceId)) //InfraBase
            {
                await _infraBaseFileUploaderService.InfraBaseRequestUpload(uploadDocumentsDto);
                return;
            }
            else if (uploadDocumentsDto.Documents != null && uploadDocumentsDto.Documents.Count > 0 && uploadDocumentsDto.RegardingObjectId == Guid.Empty) //DataCollection
            {
                await _dataCollectionService.UploadDocuments(uploadDocumentsDto);
            }
            else if (uploadDocumentsDto.Documents != null && uploadDocumentsDto.Documents.Count > 0 && !string.IsNullOrEmpty(uploadDocumentsDto.EsmRequestId)) //Commercialization
            {
                var esmUploadedDocuments = uploadDocumentsDto.Documents.Select(x=> new EsmUploadedDocument
                {
                    Bytes = Convert.FromBase64String(x.DocumentContent),
                    Name = x.DocumentName,
                    Extension = x.DocumentExtension,
                    Size = x.DocumentSize,
                    KeyName = x.FormDataKeyName

                }).ToList();
                await _commercializationAppService.CommercializationUpload(uploadDocumentsDto.EsmRequestId, esmUploadedDocuments);
            }
        }

        public void WriteToFile(string fileName, string content)
        {
            _externalFileScanningService.WriteToFile(fileName, content);
        }

        private void FillRequestDocumentComments(Guid requestDocumentId, int fildResult, string dataId)
        {
            var orgContext = new OrganizationServiceContext(_crmService.GetInstance());

            IQueryable<Entity> Query = orgContext.CreateQuery(EntityNames.RequestDocument).AsQueryable();
            Entity documentEntity = Query.Where(x => ((Guid)x["hexa_requestdocumentid"]).Equals(requestDocumentId)).FirstOrDefault();

            if (documentEntity != null)
            {
                var Entity = new Entity(EntityNames.RequestDocumentComment);

                Entity["hexa_name"] = "Commented";
                Entity["hexa_comment"] = "Please check with OPSWAT Admin, response code :" + fildResult + " ,dataid :" + dataId;

                if (documentEntity.Contains("hexa_requestdocumentid"))
                    Entity["hexa_requestdocumentid"] = new EntityReference(EntityNames.RequestDocument, new Guid(documentEntity.GetValueByAttributeName<EntityReferenceDto>("hexa_requestdocumentid").Id));
                if (documentEntity.Contains("hexa_evaluatedatstep"))
                    Entity["hexa_requeststepid"] = new EntityReference(EntityNames.RequestStep, new Guid(documentEntity.GetValueByAttributeName<EntityReferenceDto>("hexa_evaluatedatstep").Id));

                var Id = _crmService.Create(Entity, EntityNames.RequestDocumentComment);
            }
        }
    }
}
