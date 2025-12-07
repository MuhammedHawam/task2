using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using PIF.EBP.Application.Feedback.DTOs;
using PIF.EBP.Core.CRM;
using PIF.EBP.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PIF.EBP.Application.Feedback.Implementation
{
    public class FeedbackFileUploaderService: IFeedbackFileUploaderService
    {
        private readonly ICrmService _crmService;
        public FeedbackFileUploaderService(ICrmService crmService)
        {
            _crmService = crmService;
        }
        public void AttachFileToCRMRecord(EntityReference recordReference, string fileAttributeName, AttachmentAttributesDto attachmentAttributes)
        {
            byte[] fileBytes = Convert.FromBase64String(attachmentAttributes.FileContent);

            string fileExtension = "." + attachmentAttributes.FileExtension;
            string fileName = attachmentAttributes.FileName + fileExtension;
            string mimeType = MapMimeType(fileExtension?.ToLowerInvariant());

            var initializeFileUploadRequest = new InitializeFileBlocksUploadRequest
            {
                FileAttributeName = fileAttributeName,
                Target = recordReference,
                FileName = fileName
            };

            var fileUploadResponse = (InitializeFileBlocksUploadResponse)_crmService.GetInstance().Execute(initializeFileUploadRequest);

            var blockSize = 4 * 1024 * 1024; // 4 MB
            var blockIds = new List<string>();

            for (int i = 0; i < Math.Ceiling(fileBytes.Length / Convert.ToDecimal(blockSize)); i++)
            {
                var blockId = Convert.ToBase64String(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()));
                blockIds.Add(blockId);

                var blockData = fileBytes.Skip(i * blockSize).Take(blockSize).ToArray();
                var blockRequest = new UploadBlockRequest()
                {
                    FileContinuationToken = fileUploadResponse.FileContinuationToken,
                    BlockId = blockId,
                    BlockData = blockData
                };

                _crmService.GetInstance().Execute(blockRequest);
            }

            var commitRequest = new CommitFileBlocksUploadRequest()
            {
                BlockList = blockIds.ToArray(),
                FileContinuationToken = fileUploadResponse.FileContinuationToken,
                FileName = fileName,
                MimeType = mimeType
            };

            _crmService.GetInstance().Execute(commitRequest);
        }

        private string MapMimeType(string fileExtension)
        {
            string mimeType = string.Empty;

            switch (fileExtension?.ToLowerInvariant())
            {
                case ".pdf":
                    mimeType = "application/pdf";
                    break;
                case ".jpeg":
                case ".jpg":
                    mimeType = "image/jpeg";
                    break;
                case ".png":
                    mimeType = "image/png";
                    break;
                case ".txt":
                    mimeType = "text/plain";
                    break;
                case ".gif":
                    mimeType = "image/gif";
                    break;
                case ".bmp":
                    mimeType = "image/bmp";
                    break;
                case ".tiff":
                case ".tif":
                    mimeType = "image/tiff";
                    break;
                case ".doc":
                    mimeType = "application/msword";
                    break;
                case ".docx":
                    mimeType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                    break;
                case ".xls":
                    mimeType = "application/vnd.ms-excel";
                    break;
                case ".xlsx":
                    mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    break;
                case ".ppt":
                    mimeType = "application/vnd.ms-powerpoint";
                    break;
                case ".pptx":
                    mimeType = "application/vnd.openxmlformats-officedocument.presentationml.presentation";
                    break;
                case ".csv":
                    mimeType = "text/csv";
                    break;
                case ".xml":
                    mimeType = "application/xml";
                    break;
                case ".zip":
                    mimeType = "application/zip";
                    break;
                case ".rar":
                    mimeType = "application/x-rar-compressed";
                    break;
                case ".7z":
                    mimeType = "application/x-7z-compressed";
                    break;
                case ".mp3":
                    mimeType = "audio/mpeg";
                    break;
                case ".wav":
                    mimeType = "audio/wav";
                    break;
                case ".mp4":
                    mimeType = "video/mp4";
                    break;
                case ".avi":
                    mimeType = "video/x-msvideo";
                    break;
                case ".mov":
                    mimeType = "video/quicktime";
                    break;
                case ".html":
                case ".htm":
                    mimeType = "text/html";
                    break;
                case ".css":
                    mimeType = "text/css";
                    break;
                case ".js":
                    mimeType = "application/javascript";
                    break;
                case ".json":
                    mimeType = "application/json";
                    break;
                default:
                    throw new UserFriendlyException("InvalidFileExtension", System.Net.HttpStatusCode.BadRequest);
            }
            return mimeType;
        }
    }
}
