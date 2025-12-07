using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using PIF.EBP.Application.Shared;
using PIF.EBP.Application.SMTPNotificaation.DTOs;
using PIF.EBP.Core.CRM;
using PIF.EBP.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;

namespace PIF.EBP.Application.SMTPNotificaation.Implmentation
{
    public class SMTPNotificationService : ISMTPNotificationService
    {
        private readonly ICrmService _crmService;

        public SMTPNotificationService(ICrmService crmService)
        {
            _crmService = crmService ?? throw new ArgumentNullException(nameof(crmService));
        }

        public async Task<Guid> SendCrmEmailAsync(SendEmailDto emailDto)
        {
            if (emailDto == null)
                throw new ArgumentNullException(nameof(emailDto));

            if (string.IsNullOrWhiteSpace(emailDto.Subject))
                throw new ArgumentException("Email subject is required.", nameof(emailDto.Subject));

            if (string.IsNullOrWhiteSpace(emailDto.Body))
                throw new ArgumentException("Email body is required.", nameof(emailDto.Body));

            if (emailDto.ToEmails == null || !emailDto.ToEmails.Any())
                throw new ArgumentException("At least one recipient email is required.", nameof(emailDto.ToEmails));

            var crmService = _crmService.GetInstance();
            
            try
            {
                Entity email = new Entity("email");
                email["subject"] = emailDto.Subject;
                email["description"] = emailDto.Body;

                if (emailDto.IsHtmlBody)
                {
                    email["mimetype"] = "text/html";
                }

                var toParties = new List<Entity>();
                foreach (var toEmail in emailDto.ToEmails.Where(e => !string.IsNullOrWhiteSpace(e)))
                {
                    var toParty = GetPartyFromEmail(toEmail);
                    if (toParty != null)
                    {
                        toParties.Add(new Entity("activityparty") 
                        { 
                            ["partyid"] = toParty 
                        });
                    }
                }

                if (!toParties.Any())
                    throw new UserFriendlyException("NoValidRecipientsFound");

                email["to"] = toParties.ToArray();

                if (emailDto.CcEmails != null && emailDto.CcEmails.Any())
                {
                    var ccParties = new List<Entity>();
                    foreach (var ccEmail in emailDto.CcEmails.Where(e => !string.IsNullOrWhiteSpace(e)))
                    {
                        var ccParty = GetPartyFromEmail(ccEmail);
                        if (ccParty != null)
                        {
                            ccParties.Add(new Entity("activityparty") 
                            { 
                                ["partyid"] = ccParty 
                            });
                        }
                    }
                    if (ccParties.Any())
                    {
                        email["cc"] = ccParties.ToArray();
                    }
                }

                if (emailDto.BccEmails != null && emailDto.BccEmails.Any())
                {
                    var bccParties = new List<Entity>();
                    foreach (var bccEmail in emailDto.BccEmails.Where(e => !string.IsNullOrWhiteSpace(e)))
                    {
                        var bccParty = GetPartyFromEmail(bccEmail);
                        if (bccParty != null)
                        {
                            bccParties.Add(new Entity("activityparty") 
                            { 
                                ["partyid"] = bccParty 
                            });
                        }
                    }
                    if (bccParties.Any())
                    {
                        email["bcc"] = bccParties.ToArray();
                    }
                }

                Guid emailId = crmService.Create(email);

                if (emailDto.Attachments != null && emailDto.Attachments.Any())
                {
                    foreach (var attachment in emailDto.Attachments)
                    {
                        CreateAttachment(attachment, emailId);
                    }
                }

                var sendEmailRequest = new SendEmailRequest
                {
                    EmailId = emailId,
                    IssueSend = true
                };

                await Task.Run(() => crmService.Execute(sendEmailRequest)).ConfigureAwait(false);

                return emailId;
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                throw new UserFriendlyException($"CRM Error while sending email: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException($"Error while sending email: {ex.Message}");
            }
        }

        private void CreateAttachment(EmailAttachmentDto attachment, Guid emailId)
        {
            if (attachment == null)
                throw new ArgumentNullException(nameof(attachment));

            if (string.IsNullOrWhiteSpace(attachment.FileName))
                throw new ArgumentException("Attachment file name is required.", nameof(attachment.FileName));

            if (attachment.FileContent == null || attachment.FileContent.Length == 0)
                throw new ArgumentException("Attachment file content is required.", nameof(attachment.FileContent));

            try
            {
                string base64Content = Convert.ToBase64String(attachment.FileContent);

                Entity attachmentEntity = new Entity("activitymimeattachment");
                attachmentEntity["objectid"] = new EntityReference("email", emailId);
                attachmentEntity["objecttypecode"] = "email";
                attachmentEntity["filename"] = attachment.FileName;
                attachmentEntity["body"] = base64Content;
                attachmentEntity["mimetype"] = !string.IsNullOrWhiteSpace(attachment.MimeType)
                    ? attachment.MimeType
                    : GetMimeTypeFromFileName(attachment.FileName);

                _crmService.GetInstance().Create(attachmentEntity);
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException($"Error creating attachment: {ex.Message}");
            }
        }

        private string GetMimeTypeFromFileName(string fileName)
        {
            string extension = Path.GetExtension(fileName)?.ToLowerInvariant();
            switch (extension)
            {
                case ".pdf":
                    return "application/pdf";
                case ".doc":
                    return "application/msword";
                case ".docx":
                    return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                case ".xls":
                    return "application/vnd.ms-excel";
                case ".xlsx":
                    return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                case ".png":
                    return "image/png";
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".gif":
                    return "image/gif";
                case ".txt":
                    return "text/plain";
                case ".zip":
                    return "application/zip";
                default:
                    return "application/octet-stream";
            }
        }

        private EntityReference GetPartyFromEmail(string emailAddress)
        {
            if (string.IsNullOrWhiteSpace(emailAddress))
                return null;

            var crmService = _crmService.GetInstance();

            var contactQuery = new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet("contactid"),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("emailaddress1", ConditionOperator.Equal, emailAddress)
                    }
                },
                TopCount = 1
            };

            EntityCollection contactResults = crmService.RetrieveMultiple(contactQuery);
            if (contactResults.Entities.Count > 0)
            {
                return contactResults.Entities[0].ToEntityReference();
            }

            var queueQuery = new QueryExpression("queue")
            {
                ColumnSet = new ColumnSet("queueid"),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("emailaddress", ConditionOperator.Equal, emailAddress)
                    }
                },
                TopCount = 1
            };

            EntityCollection queueResults = crmService.RetrieveMultiple(queueQuery);
            if (queueResults.Entities.Count > 0)
            {
                return queueResults.Entities[0].ToEntityReference();
            }

            var userQuery = new QueryExpression("systemuser")
            {
                ColumnSet = new ColumnSet("systemuserid"),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("internalemailaddress", ConditionOperator.Equal, emailAddress)
                    }
                },
                TopCount = 1
            };

            EntityCollection userResults = crmService.RetrieveMultiple(userQuery);
            if (userResults.Entities.Count > 0)
            {
                return userResults.Entities[0].ToEntityReference();
            }

            return null;
        }
    }
}
