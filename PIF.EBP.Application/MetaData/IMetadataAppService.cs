using PIF.EBP.Application.Hexa.DTOs;
using PIF.EBP.Application.MetaData.DTOs;
using PIF.EBP.Application.Shared.AppResponse;
using PIF.EBP.Core.DependencyInjection;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PIF.EBP.Application.MetaData
{
    public interface IMetadataAppService : ITransientDependency
    {
        Task<ListPagingResponse<EntityAttributeDto>> RetrieveEntityAttributes(EntityAttributeRequestDto entityAttributeRequestDto);
        Task<List<EntityRelationshipDto>> RetrieveEntityRelationships(string entityName);
        Task<List<EntityFormDto>> RetrieveEntityForms(string entityName);
        Task<List<EntityViewDto>> RetrieveEntityViews(string entityName);
        Task<List<EntityLookupDto>> RetrieveEntityLookupValuesAsync(EntityLookupOptions options);
        Task<List<EntityLookupDto>> RetrieveCustomEntityLookupValuesAsync(string key);

        Task<List<EntityOptionSetDto>> RetrieveEntityOptionsSetValuesAsync(EntityOptionSetOptions options);
        Task<EntityGridViewDataResponse> RetrieveGridViewDataByGridId(GridViewDataReq oGridViewDataReq);
        void DeleteEntityRecordById(DeleteEntityRecordRequest deleteEntityRequest);
    }
}
