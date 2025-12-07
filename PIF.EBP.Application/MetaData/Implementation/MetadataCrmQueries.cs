using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using PIF.EBP.Application.MetaData.DTOs;
using PIF.EBP.Application.Shared;
using PIF.EBP.Core.CRM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PIF.EBP.Application.MetaData.Implementation
{
    public class MetadataCrmQueries : IMetadataCrmQueries
    {
        private readonly ICrmService _crmService;
        public MetadataCrmQueries(ICrmService crmService)
        {
            _crmService = crmService;
        }
        public async Task<IEnumerable<TCacheItem>> GetItemFromDataSource<TCacheItem, TPrimaryKey>(string entityName)
            where TCacheItem : CacheItemBase<TPrimaryKey>
        {
            var entityMetadata = _crmService.GetMetaData(entityName);
            List<EntityAttributeDto> entityAttributeDto = GetEntAttributes(entityMetadata);
            List<EntityRelationshipDto> entityRelationships = GetEntRelationships(entityName, entityMetadata);
            List<EntityFormDto> formList = GetEntForms(entityName);
            List<EntityViewDto> viewList = GetEntViews(entityName);
            var cacheItem = new MetadataCacheItem
            {
                Id = entityMetadata.MetadataId.ToString(),
                AttributeList = entityAttributeDto,
                RelationshipList=entityRelationships,
                FormList=formList,
                ViewList=viewList

            };
            return (IEnumerable<TCacheItem>)new List<MetadataCacheItem>() { cacheItem };
        }
        public async Task<List<T>> GetFromDataSource<T>(string entityName) where T : ICacheItem
        {
            IEnumerable<ICacheItem> items = Enumerable.Empty<ICacheItem>();
            if (typeof(T) == typeof(EntityAttributeDto))
            {
                var entityMetadata = _crmService.GetMetaData(entityName);
                items = GetEntAttributes(entityMetadata);
            }
            else if (typeof(T) == typeof(EntityRelationshipDto))
            {
                var entityMetadata = _crmService.GetMetaData(entityName);
                items = GetEntRelationships(entityName, entityMetadata);
            }
            else if (typeof(T) == typeof(EntityFormDto))
            {
                items = GetEntForms(entityName);
            }
            else if (typeof(T) == typeof(EntityViewDto))
            {
                items = GetEntViews(entityName);
            }
            return items.OfType<T>().ToList();
        }

        private List<EntityAttributeDto> GetEntAttributes(EntityMetadata entityMetadata)
        {
            return entityMetadata.Attributes.Where(x => x.DisplayName.LocalizedLabels.Count>0).Select(entityValue => FillEntityAttributes(entityValue)).ToList();
        }
        private List<EntityRelationshipDto> GetEntRelationships(string entityName, EntityMetadata entityMetadata)
        {
            var entityRelationships = new HashSet<(string MetadataId, string Name, int Type,string RefAttribute,string RefEntity,
                                                   string ReferencingEntity, string ReferencedEntity, string ReferencingAttribute, string ReferencedAttribute,
                                                   string Entity1IntersectAttribute, string Entity2IntersectAttribute, string Entity1LogicalName, string Entity2LogicalName,string IntersectEntityName)>();

            foreach (var relationship in entityMetadata.ManyToOneRelationships)
            {
                var dto = new EntityRelationshipDto
                {
                    MetadataId = relationship.MetadataId?.ToString(),
                    Name = relationship.SchemaName,
                    Type = (int)relationship.RelationshipType,
                    RefAttribute = relationship.ReferencedEntity.Equals(entityName) ? relationship.ReferencingAttribute : relationship.ReferencedAttribute,
                    RefEntity = relationship.ReferencedEntity.Equals(entityName) ? relationship.ReferencingEntity : relationship.ReferencedEntity,
                    ReferencingEntity=relationship.ReferencingEntity,
                    ReferencedEntity=relationship.ReferencedEntity,
                    ReferencingAttribute=relationship.ReferencingAttribute,
                    ReferencedAttribute=relationship.ReferencedAttribute,

                };

                entityRelationships.Add((dto.MetadataId, dto.Name, dto.Type,dto.RefAttribute,dto.RefEntity, dto.ReferencingEntity, dto.ReferencedEntity, dto.ReferencingAttribute, dto.ReferencedAttribute, "", "", "", "", ""));
            }

            foreach (var relationship in entityMetadata.OneToManyRelationships)
            {
                var dto = new EntityRelationshipDto
                {
                    MetadataId = relationship.MetadataId?.ToString(),
                    Name = relationship.SchemaName,
                    Type = (int)relationship.RelationshipType,
                    RefAttribute = relationship.ReferencedEntity.Equals(entityName) ? relationship.ReferencingAttribute : relationship.ReferencedAttribute,
                    RefEntity = relationship.ReferencedEntity.Equals(entityName) ? relationship.ReferencingEntity : relationship.ReferencedEntity,
                    ReferencingEntity=relationship.ReferencingEntity,
                    ReferencedEntity=relationship.ReferencedEntity,
                    ReferencingAttribute=relationship.ReferencingAttribute,
                    ReferencedAttribute=relationship.ReferencedAttribute,
                };

                entityRelationships.Add((dto.MetadataId, dto.Name, dto.Type, dto.RefAttribute, dto.RefEntity, dto.ReferencingEntity, dto.ReferencedEntity, dto.ReferencingAttribute, dto.ReferencedAttribute, "", "", "", "", ""));
            }

            foreach (var relationship in entityMetadata.ManyToManyRelationships)
            {
                var dto = new EntityRelationshipDto
                {
                    MetadataId = relationship.MetadataId?.ToString(),
                    Name = relationship.SchemaName,
                    Type = (int)relationship.RelationshipType,
                    RefAttribute = relationship.Entity1LogicalName.Equals(entityName) ? relationship.Entity2IntersectAttribute : relationship.Entity1IntersectAttribute,
                    RefEntity = relationship.Entity1LogicalName.Equals(entityName) ? relationship.Entity2LogicalName : relationship.Entity1LogicalName,
                    Entity1IntersectAttribute=relationship.Entity1IntersectAttribute,
                    Entity2IntersectAttribute=relationship.Entity2IntersectAttribute,
                    Entity1LogicalName=relationship.Entity1LogicalName,
                    Entity2LogicalName=relationship.Entity2LogicalName,
                    IntersectEntityName=relationship.IntersectEntityName

                };

                entityRelationships.Add((dto.MetadataId, dto.Name, dto.Type, dto.RefAttribute, dto.RefEntity, "", "", "", "", dto.Entity1IntersectAttribute, dto.Entity2IntersectAttribute, dto.Entity1LogicalName, dto.Entity2LogicalName,dto.IntersectEntityName));
            }

            return entityRelationships.Select(key => new EntityRelationshipDto
            {
                MetadataId = key.MetadataId,
                Name = key.Name,
                Type = key.Type,
                RefAttribute = key.RefAttribute,
                RefEntity = key.RefEntity,
                ReferencingEntity = key.ReferencingEntity,
                ReferencedEntity = key.ReferencedEntity,
                ReferencingAttribute=key.ReferencingAttribute,
                ReferencedAttribute=key.ReferencedAttribute,
                Entity1IntersectAttribute = key.Entity1IntersectAttribute,
                Entity2IntersectAttribute = key.Entity2IntersectAttribute,
                Entity1LogicalName = key.Entity1LogicalName,
                Entity2LogicalName = key.Entity2LogicalName,
                IntersectEntityName=key.IntersectEntityName
            }).ToList();
        }
        private List<EntityFormDto> GetEntForms(string entityName)
        {
            if (string.IsNullOrEmpty(entityName))
            {
                // Consider throwing an ArgumentException or returning an empty list based on your application's needs
                throw new ArgumentException("Entity name cannot be null or empty.", nameof(entityName));
            }

            var query = new QueryExpression("systemform")
            {
                ColumnSet = new ColumnSet("formxml"),
                Criteria = new FilterExpression()
            };
            query.Criteria.AddCondition("objecttypecode", ConditionOperator.Equal, entityName.ToLower());

            var entityCollection = _crmService.GetInstance().RetrieveMultiple(query);

            if (entityCollection.Entities.Any())
            {
                return entityCollection.Entities.Select(entityValue => FillEntityForms(entityValue)).ToList();
            }

            return new List<EntityFormDto>();
        }
        private List<EntityViewDto> GetEntViews(string entityName)
        {
            var orgContext = new OrganizationServiceContext(_crmService.GetInstance());

            var entityViews = from entity in orgContext.CreateQuery("savedquery")
                              where (string)entity["returnedtypecode"] == entityName
                              select CreateEntityViewDtoFromEntity(entity);

            return entityViews.ToList();
        }

        private EntityAttributeDto FillEntityAttributes(AttributeMetadata attributeMetadata)
        {
            var validations = new Dictionary<string, int>();
            var options = new Dictionary<string, int>();
            var lookupTarget = "";
            string format = string.Empty;
            switch (attributeMetadata)
            {
                case IntegerAttributeMetadata integerAttribute:
                    AddValidation(validations, integerAttribute.MaxValue, "MaxValue");
                    AddValidation(validations, integerAttribute.MinValue, "MinValue");
                    break;
                case StringAttributeMetadata stringAttribute:
                    AddValidation(validations, stringAttribute.MaxLength, "MaxLength");
                    break;
                case LookupAttributeMetadata lookupAttribute:
                    lookupTarget=lookupAttribute.Targets[0]??string.Empty;
                    break;
                case PicklistAttributeMetadata picklistAttribute:
                    AddOptionSetValues(options, picklistAttribute.OptionSet.Options.ToArray());
                    break;
                case DateTimeAttributeMetadata dateTimeAttribute:
                    format =dateTimeAttribute?.Format.Value.ToString()??string.Empty;
                    break;
            }

            return new EntityAttributeDto
            {
                MetadataId=attributeMetadata.MetadataId.ToString(),
                Name = attributeMetadata.LogicalName,
                Type = attributeMetadata.AttributeType.ToString(),
                DisplayName=attributeMetadata.DisplayName.UserLocalizedLabel?.Label??string.Empty,
                ColumnNumber=attributeMetadata.ColumnNumber,
                IsRequiredForForm=attributeMetadata.IsRequiredForForm,
                LookupTarget=lookupTarget,
                DateTimeFormat=format,
                Validations = validations,
                Options=options
            };
        }
        private EntityFormDto FillEntityForms(Entity entity)
        {
            return new EntityFormDto
            {
                FormXml= CRMUtility.GetAttributeValue(entity, "formxml",string.Empty),
                Id=entity.Id.ToString(),
            };
        }
        private EntityViewDto CreateEntityViewDtoFromEntity(Entity entity)
        {
            return new EntityViewDto
            {
                Name = entity.GetAttributeValue<string>("name"),
                Id = entity.GetAttributeValue<Guid>("savedqueryid").ToString(),
                IsDefault = entity.GetAttributeValue<bool>("isdefault"),
                IsManaged = entity.GetAttributeValue<bool>("ismanaged"),
                QueryType=entity.GetAttributeValue<int>("querytype"),
                StatusCode=CRMUtility.GetOptionSetValue(entity,"statuscode")
            };
        }

        private void AddValidation(Dictionary<string, int> validations, int? value, string validationType)
        {
            if (value.HasValue && !validations.ContainsValue(value.Value))
            {
                validations.Add(validationType, value.Value);
            }
        }
        private void AddOptionSetValues(Dictionary<string, int> options, OptionMetadata[] optionList)
        {
            if (optionList != null)
            {
                int cultureCode = CRMUtility.GetCultureForCRM("en");
                foreach (OptionMetadata option in optionList)
                {
                    var localizedLabel = option.Label.LocalizedLabels.FirstOrDefault(x => x.LanguageCode == cultureCode)?.Label ??
                                         option.Label.LocalizedLabels.FirstOrDefault()?.Label;

                    if (option.Value.HasValue && !string.IsNullOrWhiteSpace(localizedLabel))
                    {
                        options.Add(localizedLabel, option.Value.Value);
                    }
                }
            }
        }

        
    }
}
