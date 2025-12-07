using Microsoft.Xrm.Sdk;
using PIF.EBP.Application.Feedback.DTOs;
using PIF.EBP.Core.DependencyInjection;

namespace PIF.EBP.Application.Feedback
{
    public interface IFeedbackFileUploaderService : ITransientDependency
    {
        void AttachFileToCRMRecord(EntityReference recordReference, string fileAttributeName, AttachmentAttributesDto attachmentAttributes);
    }
}
