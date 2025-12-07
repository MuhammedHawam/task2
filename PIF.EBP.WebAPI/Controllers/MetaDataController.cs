using PIF.EBP.Application.Hexa.DTOs;
using PIF.EBP.Application.MetaData;
using PIF.EBP.Application.MetaData.DTOs;
using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.Core.Exceptions;
using PIF.EBP.WebAPI.Middleware.ActionFilter;
using System;
using System.Threading.Tasks;
using System.Web.Http;

namespace PIF.EBP.WebAPI.Controllers
{
    [ApiResponseWrapper]
    [RoutePrefix("MetaData")]
    public class MetaDataController : BaseController
    {
        private readonly IMetadataAppService _metadataAppService;
        public MetaDataController()
        {
            _metadataAppService = WindsorContainerProvider.Container.Resolve<IMetadataAppService>();
        }

        [HttpPost]
        [Route("get-entity-attributes")]
        public async Task<IHttpActionResult> GetEntityAttributesByEntityName(EntityAttributeRequestDto entityAttributeRequestDto)
        {
            var result = await _metadataAppService.RetrieveEntityAttributes(entityAttributeRequestDto);

            return Ok(result);
        }
        [HttpGet]
        [Route("get-entity-relationships")]
        public async Task<IHttpActionResult> GetEntityRelationshipsByEntityName(string entityName)
        {
            var result = await _metadataAppService.RetrieveEntityRelationships(entityName);

            return Ok(result);
        }
        [HttpGet]
        [Route("get-entity-forms")]
        public async Task<IHttpActionResult> GetEntityFormsByEntityName(string entityName)
        {
            var result = await _metadataAppService.RetrieveEntityForms(entityName);

            return Ok(result);
        }
        [HttpGet]
        [Route("get-entity-views")]
        public async Task<IHttpActionResult> GetEntityViewsByEntityName(string entityName)
        {
            var result = await _metadataAppService.RetrieveEntityViews(entityName);

            return Ok(result);
        }

        [HttpPost]
        [Route("get-entity-lookup-values")]
        public async Task<IHttpActionResult> GetEntityLookupValues(EntityLookupOptions options)
        {
            var result = await _metadataAppService.RetrieveEntityLookupValuesAsync(options);

            return Ok(result);
        }

        [HttpGet]
        [Route("get-custom-entity-lookup-values")]
        public async Task<IHttpActionResult> GetCustomEntityLookupValues(string key)
        {
            var result = await _metadataAppService.RetrieveCustomEntityLookupValuesAsync(key);

            return Ok(result);
        }


        [HttpGet]
        [Route("get-entity-optionset-values")]
        public async Task<IHttpActionResult> GetEntityOptionSetValues([FromUri] EntityOptionSetOptions options)
        {
            if (options == null || string.IsNullOrWhiteSpace(options.EntityName) || string.IsNullOrWhiteSpace(options.AttributeName))
            {
                throw new UserFriendlyException("RequiredParameters");
            }

            try
            {
                // Assuming RetrieveEntityLookupsAsync is the asynchronous version of RetrieveEntityLookups
                var result = await _metadataAppService.RetrieveEntityOptionsSetValuesAsync(options);

                return Ok(result); // Directly return the result. Web API serializes it to JSON.
            }
            catch (Exception ex)
            {
                // Log the exception details here as necessary
                return InternalServerError(ex); // Provide a generic error response to avoid leaking sensitive information
            }
        }

        [HttpPost]
        [Route("get-gridview-data")]
        public async Task<IHttpActionResult> GetGridViewDataByGridId(GridViewDataReq oGridViewDataReq)
        {
            var result = await _metadataAppService.RetrieveGridViewDataByGridId(oGridViewDataReq);

            return Ok(result);
        }

        [HttpDelete]
        [Route("delete-entity-record")]
        public IHttpActionResult DeleteEntityRecordById(DeleteEntityRecordRequest deleteEntityRequest)
        {
            _metadataAppService.DeleteEntityRecordById(deleteEntityRequest);

            return Ok();
        }

    }
}
