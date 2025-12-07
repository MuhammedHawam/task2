using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk;
using PIF.EBP.Application.ExternalFormConfiguration.DTOs;
using PIF.EBP.Application.Shared;
using PIF.EBP.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using PIF.EBP.Application.MetaData.DTOs;
using PIF.EBP.Application.Shared.Helpers;
using PIF.EBP.Core.CRM;
using Microsoft.Xrm.Sdk.Query;

namespace PIF.EBP.Application.ExternalFormConfiguration.Implementation
{
    public class ExternalFormConfigAppService : IExternalFormConfigAppService
    {
        private readonly ICrmService _crmService;

        public ExternalFormConfigAppService(ICrmService crmService)
        {
            _crmService = crmService;
        }

        public List<ExternalFormConfigDto> RetrieveExternalFormConfiguration(Guid? Id = null, int? Type = null)
        {
            var externalFormConfigs = new List<ExternalFormConfigDto>();
            var orgContext = new OrganizationServiceContext(_crmService.GetInstance());

            var query = new QueryExpression(EntityNames.ExternalFormConfiguration)
            {
                ColumnSet = new ColumnSet("hexa_typetypecode", "pwc_gridtargetentity", "hexa_externalformconfigurationid", "hexa_name", "hexa_grids", "pwc_selectedentities", "hexa_formmetadata", "hexa_actions", "pwc_formtypetypecode", "hexa_processtemplateid", "hexa_processsteptemplateid"),
                
            };

            LinkEntity processTemplateLink = new LinkEntity(EntityNames.ExternalFormConfiguration, EntityNames.ProcessTemplate, "hexa_processtemplateid", "hexa_processtemplateid", JoinOperator.LeftOuter)
            {
                Columns = new ColumnSet("hexa_nameen", "hexa_namear"),
                EntityAlias = "processtemplateAlias"
            };

            query.LinkEntities.Add(processTemplateLink);


            if (!(Id == null || Id == Guid.Empty))
            {
                query.Criteria.AddCondition("hexa_externalformconfigurationid", ConditionOperator.Equal, Id.Value);
            }

            if (Type != null && int.TryParse(Type.ToString(), out int type))
            {
                query.Criteria.AddCondition("hexa_typetypecode", ConditionOperator.Equal, type);
            }
            var result = _crmService.GetInstance().RetrieveMultiple(query);


            externalFormConfigs.AddRange(result.Entities.Select(entity => FillExternalFormConfig(entity)).ToList());

            return externalFormConfigs;
        }

        public ExternalFormConfigDto CreateExternalFormConfiguration(ExternalFormConfigDto ExternalFormConfigDto)
        {
            var Entity = new Entity(EntityNames.ExternalFormConfiguration);

            Entity["hexa_name"] = ExternalFormConfigDto.Name;
            Entity["hexa_grids"] = ExternalFormConfigDto.Grids;
            Entity["hexa_formmetadata"] = ExternalFormConfigDto.Metadata;
            Entity["pwc_selectedentities"] = ExternalFormConfigDto.SelectedEntities;
            Entity["hexa_actions"] = ExternalFormConfigDto.Actions;

            if (ExternalFormConfigDto.ProcessTemplate != null)
                Entity["hexa_processtemplateid"] = new EntityReference(EntityNames.ProcessTemplate, new Guid(ExternalFormConfigDto.ProcessTemplate.Id));
            if (ExternalFormConfigDto.ProcessStepTemplate != null)
                Entity["hexa_processsteptemplateid"] = new EntityReference(EntityNames.ProcessStepTemplate, new Guid(ExternalFormConfigDto.ProcessStepTemplate.Id));

            if (ExternalFormConfigDto.Type != null)
            {
                if (int.TryParse(ExternalFormConfigDto.Type.Value, out int optionSetValue))
                {
                    Entity["hexa_typetypecode"] = new OptionSetValue(optionSetValue);
                }
                else
                {
                    throw new InvalidCastException("The Value for the type code must be a valid integer.");
                }
            }

            var Id = _crmService.Create(Entity, EntityNames.ExternalFormConfiguration);
            if (!(Id == null || Id == Guid.Empty))
            {
                var externalFormConfigs = RetrieveExternalFormConfiguration(Id);
                return externalFormConfigs.FirstOrDefault();
            }
            return null;
        }

        public void UpdateExternalFormConfiguration(ExternalFormConfigDto ExternalFormConfigDto)
        {
            var Entity = new Entity(EntityNames.ExternalFormConfiguration);

            Entity.Id = ExternalFormConfigDto.Id;
            Entity["hexa_externalformconfigurationid"] = ExternalFormConfigDto.Id;
            if (!string.IsNullOrEmpty(ExternalFormConfigDto.Name))
            {
                Entity["hexa_name"] = ExternalFormConfigDto.Name;
            }

            Entity["hexa_grids"] = ExternalFormConfigDto.Grids;
            Entity["hexa_formmetadata"] = ExternalFormConfigDto.Metadata;
            Entity["pwc_selectedentities"] = ExternalFormConfigDto.SelectedEntities;
            Entity["hexa_actions"] = ExternalFormConfigDto.Actions;

            if (ExternalFormConfigDto.ProcessTemplate != null)
                Entity["hexa_processtemplateid"] = new EntityReference(EntityNames.ProcessTemplate, new Guid(ExternalFormConfigDto.ProcessTemplate.Id));
            if (ExternalFormConfigDto.ProcessStepTemplate != null)
                Entity["hexa_processsteptemplateid"] = new EntityReference(EntityNames.ProcessStepTemplate, new Guid(ExternalFormConfigDto.ProcessStepTemplate.Id));

            if (ExternalFormConfigDto.Type != null)
            {
                if (int.TryParse(ExternalFormConfigDto.Type.Value, out int optionSetValue))
                {
                    Entity["hexa_typetypecode"] = new OptionSetValue(optionSetValue);
                }
                else
                {
                    throw new UserFriendlyException("TheValueForTheTypeCodeMustBeValidInteger", System.Net.HttpStatusCode.BadRequest);
                }
            }

            _crmService.Update(Entity, EntityNames.ExternalFormConfiguration);
        }

        public void DeleteExternalFormConfiguration(string Id)
        {
            _crmService.Delete(Id, EntityNames.ExternalFormConfiguration);
        }

        public List<ExternalFormConfigDto> GetExternalFormConfigurations()
        {
            var query = new QueryExpression(EntityNames.ExternalFormConfiguration)
            {
                ColumnSet = new ColumnSet("hexa_externalformconfigurationid", "hexa_name", "hexa_grids", "pwc_selectedentities",
                "hexa_formmetadata", "hexa_actions", "hexa_typetypecode", "pwc_formtypetypecode", "hexa_processtemplateid", "hexa_processsteptemplateid"),
                Criteria = new FilterExpression()
            };

            LinkEntity processTemplateLink = new LinkEntity(EntityNames.ExternalFormConfiguration, EntityNames.ProcessTemplate, "hexa_processtemplateid", "hexa_processtemplateid", JoinOperator.LeftOuter)
            {
                Columns = new ColumnSet("hexa_nameen", "hexa_namear"),
                EntityAlias = "processtemplateAlias"
            };

            query.LinkEntities.Add(processTemplateLink);

            var entityCollection = _crmService.GetInstance().RetrieveMultiple(query);

            if (entityCollection.Entities.Any())
            {
                return entityCollection.Entities.Select(entityValue => FillExternalFormConfig(entityValue)).ToList();
            }

            return new List<ExternalFormConfigDto>();
        }

        private ExternalFormConfigDto FillExternalFormConfig(Entity entity)
        {
            string gridEntity = string.Empty;
            var type = entity.GetValueByAttributeName<EntityOptionSetDto>("hexa_typetypecode");
            if (type != null && type.Value=="2")//Relationship Grid
            {
                gridEntity= entity.GetValueByAttributeName<string>("pwc_gridtargetentity");
            }
            var processTemplate = entity.GetValueByAttributeName<EntityReferenceDto>("hexa_processtemplateid", "processtemplateAlias.hexa_namear");
            if(processTemplate != null)
            {
                processTemplate.Name = entity.GetValueByAttributeName<AliasedValue>("processtemplateAlias.hexa_nameen")?.Value.ToString();
            }
            return new ExternalFormConfigDto
            {
                Id = entity.GetValueByAttributeName<Guid>("hexa_externalformconfigurationid"),
                Name = entity.GetValueByAttributeName<string>("hexa_name"),
                Grids = entity.GetValueByAttributeName<string>("hexa_grids"),
                SelectedEntities = entity.GetValueByAttributeName<string>("pwc_selectedentities"),
                Metadata = entity.GetValueByAttributeName<string>("hexa_formmetadata"),
                Actions = entity.GetValueByAttributeName<string>("hexa_actions"),
                Type =type,
                FormType = entity.GetValueByAttributeName<EntityOptionSetDto>("pwc_formtypetypecode"),
                GridTargetEntity= gridEntity,
                ProcessTemplate = processTemplate,
                ProcessStepTemplate = entity.GetValueByAttributeName<EntityReferenceDto>("hexa_processsteptemplateid")
            };
        }
    }
}
