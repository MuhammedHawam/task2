using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.Core.FileManagement.DTOs;
using System;
using System.Threading.Tasks;

namespace PIF.EBP.Application.KnowledgeHub
{
    public interface IKnowledgeHubFileUploaderService: ITransientDependency
    {
        Task KnowledgeItemUpload(UploadDocumentsDto uploadDocumentsDto);
        Task<bool> AddKnowledgeHubFolders(Guid knowledgeItemId);
    }
}
