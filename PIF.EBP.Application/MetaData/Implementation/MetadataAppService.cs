using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using PIF.EBP.Application.Hexa.DTOs;
using PIF.EBP.Application.MetaData.DTOs;
using PIF.EBP.Application.PerfomanceDashboard;
using PIF.EBP.Application.Shared;
using PIF.EBP.Application.Shared.AppResponse;
using PIF.EBP.Application.Shared.Helpers;
using PIF.EBP.Core.CRM;
using PIF.EBP.Core.Exceptions;
using PIF.EBP.Core.Session;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;


namespace PIF.EBP.Application.MetaData.Implementation
{
    public class MetadataAppService : IMetadataAppService
    {
        private readonly ICrmService _crmService;
        private readonly ISessionService _sessionService;
        private readonly IMetadataCacheManager _metadataCacheManager;
        private readonly IPerformanceDashboardAppService _performanceDashboardAppService;
        public MetadataAppService(ICrmService crmService,
            ISessionService sessionService,
            IMetadataCacheManager metadataCacheManager,
            IPerformanceDashboardAppService performanceDashboardAppService)
        {
            _sessionService = sessionService;
            _crmService = crmService;
            _metadataCacheManager = metadataCacheManager;
            _performanceDashboardAppService = performanceDashboardAppService;
        }

        public async Task<ListPagingResponse<EntityAttributeDto>> RetrieveEntityAttributes(EntityAttributeRequestDto entityAttributeRequestDto)
        {
            ListPagingResponse<EntityAttributeDto> oResponse = new ListPagingResponse<EntityAttributeDto>();

            if (entityAttributeRequestDto == null || string.IsNullOrWhiteSpace(entityAttributeRequestDto.EntityName))
            {
                throw new UserFriendlyException("EntityNameCannotNullOrWhitespace");
            }

            var pageNo = entityAttributeRequestDto.PagingRequest.PageNo;
            int pageSize = entityAttributeRequestDto.PagingRequest.PageSize;
            var cachedItem = await _metadataCacheManager.GetCachedEntityAttributesAsync(entityAttributeRequestDto.EntityName);
            if (!string.IsNullOrWhiteSpace(entityAttributeRequestDto.SearchTerm))
            {
                cachedItem=cachedItem.Where(x => x.Name.ToLower().Contains(entityAttributeRequestDto.SearchTerm.ToLower()) ||
                                            x.DisplayName.ToLower().Contains(entityAttributeRequestDto.SearchTerm.ToLower())).ToList();
            }
            // Sort by Name
            var sortedItems = cachedItem.OrderBy(item => item.Name).ToList();

            oResponse.TotalCount = sortedItems.Count;
            var paginatedItems = sortedItems.Skip((pageNo - 1) * pageSize).Take(pageSize).ToList();

            oResponse.ListResponse = paginatedItems;
            return oResponse;
        }
        public async Task<List<EntityRelationshipDto>> RetrieveEntityRelationships(string entityName)
        {
            if (string.IsNullOrWhiteSpace(entityName))
            {
                throw new UserFriendlyException("EntityNameCannotNullOrWhitespace");
            }

            var cachedItem = await _metadataCacheManager.GetCachedEntityRelationshipsAsync(entityName);


            return cachedItem;
        }

        public async Task<List<EntityFormDto>> RetrieveEntityForms(string entityName)
        {
            if (string.IsNullOrEmpty(entityName))
            {
                // Consider throwing an ArgumentException or returning an empty list based on your application's needs
                throw new UserFriendlyException("EntityNameCannotNullOrWhitespace");
            }

            var cachedItem = await _metadataCacheManager.GetCachedEntityFormsAsync(entityName);
            return cachedItem;
        }

        public async Task<List<EntityViewDto>> RetrieveEntityViews(string entityName)
        {
            if (string.IsNullOrWhiteSpace(entityName))
            {
                throw new UserFriendlyException("EntityNameCannotNullOrWhitespace");
            }

            var cachedItem = await _metadataCacheManager.GetCachedEntityViewsAsync(entityName);
            return cachedItem;
        }

        public async Task<List<EntityLookupDto>> RetrieveEntityLookupValuesAsync(EntityLookupOptions options)
        {
            return await Task.Run(() =>
            {
                var primaryName = "";
                if (string.IsNullOrWhiteSpace(options.AttributeName))
                {
                    var orgContext = new OrganizationServiceContext(_crmService.GetInstance());
                    RetrieveEntityRequest retrieveAttributeRequest = new RetrieveEntityRequest()
                    {
                        LogicalName = options.EntityName
                    };

                    RetrieveEntityResponse entityResponse =
                        (RetrieveEntityResponse)orgContext.Execute(retrieveAttributeRequest);

                    options.AttributeName = entityResponse.EntityMetadata.PrimaryIdAttribute;
                    primaryName= entityResponse.EntityMetadata.PrimaryNameAttribute;
                }
                if (string.IsNullOrEmpty(options.EnColumn))
                {
                    if (string.IsNullOrEmpty(primaryName))
                    {
                        var orgContext = new OrganizationServiceContext(_crmService.GetInstance());
                        RetrieveEntityRequest retrieveAttributeRequest = new RetrieveEntityRequest()
                        {
                            LogicalName = options.EntityName
                        };

                        RetrieveEntityResponse entityResponse =
                            (RetrieveEntityResponse)orgContext.Execute(retrieveAttributeRequest);

                        options.EnColumn= entityResponse.EntityMetadata.PrimaryNameAttribute;
                    }
                    else
                    {
                        options.EnColumn = primaryName;
                    }
                    
                }

                var cols = new List<string> { options.AttributeName, options.EnColumn };
                if (!string.IsNullOrEmpty(options.ArColumn))
                {
                    cols.Add(options.ArColumn);
                }
                var query = new QueryExpression(options.EntityName)
                {
                    ColumnSet = new ColumnSet(cols.ToArray())
                };
                if (!string.IsNullOrEmpty(options.Value))
                {
                    query.Criteria.AddCondition(options.AttributeName, ConditionOperator.Equal, options.Value);
                }
                if (options.EntityName.Trim().ToLower() == EntityNames.Account)
                {
                    var comapnyFilter = new FilterExpression(LogicalOperator.Or);
                    comapnyFilter.AddCondition("pwc_investor", ConditionOperator.Equal, _sessionService.GetCompanyId());
                    comapnyFilter.AddCondition("accountid", ConditionOperator.Equal, _sessionService.GetCompanyId());
                    query.Criteria.AddFilter(comapnyFilter);
                }
                if (!string.IsNullOrEmpty(options.CompanyColumn))
                {
                    query.Criteria.AddCondition(options.CompanyColumn, ConditionOperator.Equal, _sessionService.GetCompanyId());
                }
                if (!string.IsNullOrEmpty(options.ContactColumn))
                {
                    query.Criteria.AddCondition(options.ContactColumn, ConditionOperator.Equal, _sessionService.GetContactId());
                }
                if (options.ViewId != null && options.ViewId !=Guid.Empty)
                {
                    var oEntityGridViewData = GetSavedQueryByViewId(options.ViewId.ToString());
                    if (oEntityGridViewData != null)
                    {
                        ExtractAndApplyConditionsFromFetchXml(oEntityGridViewData.FetchXml, query);
                    }
                }
                var entityCollection = _crmService.GetInstance().RetrieveMultiple(query);

                return entityCollection.Entities.Any() ?
                entityCollection.Entities.Select(entity => new EntityLookupDto
                {
                    Name =CRMOperations.GetValueByAttributeName<string>(entity, options.EnColumn),
                    NameAr =CRMOperations.GetValueByAttributeName<string>(entity, options.ArColumn),
                    Value = entity.GetAttributeValue<Guid>(options.AttributeName).ToString()
                }).ToList() :
                new List<EntityLookupDto>();
            });
        }
        public async Task<List<EntityOptionSetDto>> RetrieveEntityOptionsSetValuesAsync(EntityOptionSetOptions options)
        {
            // Assuming GetEntityOptionsSetValuesAsync is the asynchronous version that returns Task<Dictionary<int, string>>
            var records = await GetEntityOptionsSetValuesAsync(options) ?? new List<EntityOptionSetDto>();

            return records.ToList();
        }
        public async Task<List<EntityLookupDto>> RetrieveCustomEntityLookupValuesAsync(string key)
        {
            if(key.ToLower() == "company")
            {
                var myCompany = await _performanceDashboardAppService.GetMyCompany();
                var myCompanyLookupDto = new EntityLookupDto
                {
                    Name = myCompany.Name,
                    NameAr = myCompany.NameAr,
                    Value = myCompany.Id.ToString()
                };
                var companies = await _performanceDashboardAppService.GetCompaniesList(string.Empty, _sessionService.GetCompanyId(),
                    1, 5000);

                var companiesLookupDto = companies.Select(x => new EntityLookupDto
                {
                    Name = x.Name,
                    Value = x.Id.ToString(),
                    NameAr = x.NameAr
                }).ToList();

                companiesLookupDto.Insert(0, myCompanyLookupDto);

                return companiesLookupDto;
            }
            else
            {
                throw new UserFriendlyException("InvalidKey");
            }
        }

        public async Task<EntityGridViewDataResponse> RetrieveGridViewDataByGridId(GridViewDataReq oGridViewDataReq)
        {
            EntityGridViewDataResponse oResponse = new EntityGridViewDataResponse();
            var oEntityGridViewData = GetSavedQueryByViewId(oGridViewDataReq.GridId);
            if (oEntityGridViewData != null)
            {
                if (!string.IsNullOrEmpty(oEntityGridViewData.LayoutXml) && !string.IsNullOrEmpty(oEntityGridViewData.EntityName))
                {
                    var cols = GetCellsFromXml(oEntityGridViewData.LayoutXml);
                    var cachedItem = await _metadataCacheManager.GetCachedEntityAttributesAsync(oEntityGridViewData.EntityName);
                    var attributeList = cachedItem.Where(x => cols.Contains(x.Name)).Select(z => new EntityGridViewColumn
                    {
                        Key = z.Name,
                        Name = z.DisplayName
                    }).ToList();
                    oResponse.Columns=attributeList;
                }
                string primaryIdAttribute = GetPrimaryKeyName(oEntityGridViewData.EntityName);
                int pageNumber = oGridViewDataReq.PagingRequest.PageNo;
                int pageSize = oGridViewDataReq.PagingRequest.PageSize;

                var oEntityRelationship = await GetRelationshipByName(oGridViewDataReq.EntityName, oGridViewDataReq.RelationshipName);

                if (oEntityRelationship.Type== (int)Microsoft.Crm.Sdk.Messages.RelationshipType.ManyToManyRelationship)
                {
                    oEntityGridViewData.FetchXml = AddLinkEntityToFetchXmlForManyToMany(oEntityGridViewData.FetchXml, oEntityRelationship, oGridViewDataReq);
                }
                else
                {
                    oEntityGridViewData.FetchXml = AddLinkEntityToFetchXml(oEntityGridViewData.FetchXml, oEntityRelationship, oGridViewDataReq.RegardingId);
                }

                if (!string.IsNullOrEmpty(oGridViewDataReq.CompanyColumn))
                {
                    oEntityGridViewData.FetchXml=AddConditionIfNeeded(oEntityGridViewData.FetchXml, oGridViewDataReq.CompanyColumn, _sessionService.GetCompanyId());
                }
                if (!string.IsNullOrEmpty(oGridViewDataReq.ContactColumn))
                {
                    oEntityGridViewData.FetchXml=AddConditionIfNeeded(oEntityGridViewData.FetchXml, oGridViewDataReq.ContactColumn, _sessionService.GetContactId());
                }
                // Retrieve data with paging
                var dataEntityCollection = _crmService.RetrievePagingRecordsUsingFetchXml(oEntityGridViewData.FetchXml, pageNumber, pageSize, oGridViewDataReq.PagingRequest.SortField, (int)oGridViewDataReq.PagingRequest.SortOrder);
                var data = ConvertEntityCollectionToListOfDynamics(dataEntityCollection, primaryIdAttribute);
                oResponse.Rows=data;

                // Retrieve total record count
                int totalCount = dataEntityCollection.TotalRecordCount;

                oResponse.TotalCount = totalCount;
            }

            return oResponse;
        }

        public void DeleteEntityRecordById(DeleteEntityRecordRequest deleteEntityRequest)
        {
            try
            {
                var setStateRequest = new SetStateRequest
                {
                    EntityMoniker = new EntityReference(deleteEntityRequest.EntityName, new Guid(deleteEntityRequest.Id)),
                    State = new OptionSetValue(1),
                    Status = new OptionSetValue(2),
                };

                _crmService.GetInstance().Execute(setStateRequest);
            }
            catch (Exception) 
            {
                throw new UserFriendlyException("MsgUnexpectedError");
            }
        }

        private EntityGridViewData GetSavedQueryByViewId(string viewId)
        {
            if (string.IsNullOrWhiteSpace(viewId))
            {
                throw new ArgumentException("Grid Id cannot be null or whitespace.", nameof(viewId));
            }
            var query = new QueryExpression(EntityNames.SavedQuery)
            {
                ColumnSet = new ColumnSet("savedqueryid", "name", "fetchxml", "returnedtypecode", "layoutxml"),
                Criteria = new FilterExpression()
            };
            query.Criteria.AddCondition("savedqueryid", ConditionOperator.Equal, new Guid(viewId));

            var entityCollection = _crmService.GetInstance().RetrieveMultiple(query);

            if (entityCollection.Entities.Any())
            {
                var oEntityGridViewData = entityCollection.Entities.Select(entityValue => FillEntityView(entityValue)).First();
                return oEntityGridViewData;
            }
            return null;
        }

        private async Task<List<EntityOptionSetDto>> GetEntityOptionsSetValuesAsync(EntityOptionSetOptions options)
        {
            return await Task.Run(() =>
            {
                var orgContext = new OrganizationServiceContext(_crmService.GetInstance());
                var optionSetValues = new List<EntityOptionSetDto>();

                var retrieveAttributeRequest = new RetrieveAttributeRequest
                {
                    EntityLogicalName = options.EntityName,
                    LogicalName = options.AttributeName,
                    RetrieveAsIfPublished = true
                    /*
                     RetrieveAsIfPublished = “True” means return the metadata(Entity, Attributes, Relationship, Optionset, option) including unpublished changes.
                     RetrieveAsIfPublished = “False” means return only the currently published Metadata, ignoring any unpublished changes.
                     */
                };

                AttributeMetadata attributeMetadata;

                var response = (RetrieveAttributeResponse)orgContext.Execute(retrieveAttributeRequest);
                attributeMetadata = response.AttributeMetadata;

                OptionMetadata[] optionList = null;
                if (attributeMetadata is StatusAttributeMetadata statusMetadata)
                {
                    optionList = statusMetadata.OptionSet.Options.ToArray();
                }
                else if (attributeMetadata is StateAttributeMetadata stateMetadata)
                {
                    optionList = stateMetadata.OptionSet.Options.ToArray();
                }
                else if (attributeMetadata is MultiSelectPicklistAttributeMetadata multiSelectMetadata)
                {
                    optionList = multiSelectMetadata.OptionSet.Options.ToArray();
                }
                else if (attributeMetadata is PicklistAttributeMetadata picklistMetadata)
                {
                    optionList = picklistMetadata.OptionSet.Options.ToArray();
                }

                if (optionList != null)
                {
                    int cultureCode_en = CRMUtility.GetCultureForCRM(Constants.LangEn);
                    int cultureCode_ar = CRMUtility.GetCultureForCRM(Constants.LangAr);
                    foreach (OptionMetadata option in optionList)
                    {
                        var localizedLabel = option.Label.LocalizedLabels.FirstOrDefault(x => x.LanguageCode == cultureCode_en)?.Label ??
                                             option.Label.LocalizedLabels.FirstOrDefault()?.Label;
                        var localizedLabel_ar = option.Label.LocalizedLabels.FirstOrDefault(x => x.LanguageCode == cultureCode_ar)?.Label ??
                                             option.Label.LocalizedLabels.FirstOrDefault()?.Label;

                        if (option.Value.HasValue && !string.IsNullOrWhiteSpace(localizedLabel))
                        {
                            optionSetValues.Add(new EntityOptionSetDto
                            {
                                Name = localizedLabel,
                                NameAr = localizedLabel_ar,
                                Value = option.Value.Value.ToString()
                            });
                        }
                    }
                }

                return optionSetValues.OrderBy(z => z.Name).ToList();
            });
        }

        private EntityGridViewData FillEntityView(Entity entity)
        {
            return new EntityGridViewData
            {
                Id=entity.Id.ToString(),
                Name=CRMUtility.GetAttributeValue(entity, "name", string.Empty),
                FetchXml= CRMUtility.GetAttributeValue(entity, "fetchxml", string.Empty),
                LayoutXml= CRMUtility.GetAttributeValue(entity, "layoutxml", string.Empty),
                EntityName= CRMUtility.GetAttributeValue(entity, "returnedtypecode", string.Empty),
            };
        }

        private List<dynamic> ConvertEntityCollectionToListOfDynamics(EntityCollection entities,string primaryIdAttribute)
        {
            List<dynamic> resultList = new List<dynamic>();

            foreach (var entity in entities.Entities)
            {
                dynamic expandoObj = new ExpandoObject();
                var expandoDict = (IDictionary<string, object>)expandoObj;

                foreach (var attribute in entity.Attributes)
                {
                    if (attribute.Key==primaryIdAttribute)
                    {
                        expandoDict["Id"] = attribute.Value;
                    }
                    else if (attribute.Value is OptionSetValue)
                    {
                        expandoDict[attribute.Key] = entity.FormattedValues.ContainsKey(attribute.Key) ? entity.FormattedValues[attribute.Key] : string.Empty;
                    }
                    else
                    {
                        expandoDict[attribute.Key] = attribute.Value;
                    }
                }

                resultList.Add(expandoObj);
            }

            return resultList;
        }

        private List<string> GetCellsFromXml(string layoutXml)
        {
            List<string> cellNames = new List<string>();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(layoutXml);
            
            XmlNodeList cellNodes = xmlDoc.SelectNodes("//cell");

            foreach (XmlNode cellNode in cellNodes)
            {
                XmlAttribute nameAttr = cellNode.Attributes["name"];
                if (nameAttr != null)
                {
                    cellNames.Add(nameAttr.Value);
                }
            }
            return cellNames;
        }
        
        private string GetPrimaryKeyName(string entityName)
        {
            var orgContext = new OrganizationServiceContext(_crmService.GetInstance());
            RetrieveEntityRequest retrieveAttributeRequest = new RetrieveEntityRequest()
            {
                LogicalName = entityName
            };

            RetrieveEntityResponse entityResponse =
                (RetrieveEntityResponse)orgContext.Execute(retrieveAttributeRequest);

            return entityResponse.EntityMetadata.PrimaryIdAttribute;
        }

        private async Task<EntityRelationshipDto> GetRelationshipByName(string entityName,string relationshipName)
        {
            var relationshipList = await _metadataCacheManager.GetCachedEntityRelationshipsAsync(entityName);
            var oEntityRelationship = relationshipList.FirstOrDefault(x => x.Name==relationshipName);
            return oEntityRelationship;
        }

        private string AddConditionIfNeeded(string fetchXml, string columnName, string value)
        {
            XmlDocument fetchXmlDoc = new XmlDocument();
            fetchXmlDoc.LoadXml(fetchXml);


            XmlNode mainEntity = fetchXmlDoc.SelectSingleNode("//entity");

            // Check if this entity (or link-entity) already has a companyCol condition
            bool hasColumnCondition = mainEntity.SelectSingleNode($"filter/condition[@attribute='{columnName}']") != null;

            // If columnName condition is missing, add it
            if (!hasColumnCondition)
            {
                // Find the filter node or create one if it doesn't exist
                XmlNode filterNode = mainEntity.SelectSingleNode("filter");
                if (filterNode == null)
                {
                    // Create a new filter node
                    filterNode = mainEntity.OwnerDocument.CreateElement("filter");
                    ((XmlElement)filterNode).SetAttribute("type", "and");
                    mainEntity.AppendChild(filterNode);
                }

                // Create the columnName condition node
                XmlDocument doc = mainEntity.OwnerDocument;
                XmlElement conditionElement = doc.CreateElement("condition");
                conditionElement.SetAttribute("attribute", columnName);
                conditionElement.SetAttribute("operator", "eq");
                conditionElement.SetAttribute("value", value);

                // Add the condition to the filter node
                filterNode.AppendChild(conditionElement);
            }
            return fetchXmlDoc.OuterXml;

        }
        private string AddLinkEntityToFetchXml(string fetchXml, EntityRelationshipDto oEntityRelationship, string regardingId)
        {
            // Create an XmlDocument and load the existing FetchXML
            XmlDocument fetchXmlDoc = new XmlDocument();
            fetchXmlDoc.LoadXml(fetchXml);

            // Get the root <entity> node where we will append the link-entity
            XmlNode entityNode = fetchXmlDoc.SelectSingleNode("//fetch/entity");

            if (entityNode == null)
            {
                throw new InvalidOperationException("Invalid FetchXML. <entity> node not found.");
            }

            // Create <link-entity> element
            XmlElement linkEntityElement = fetchXmlDoc.CreateElement("link-entity");
            linkEntityElement.SetAttribute("name", oEntityRelationship.ReferencedEntity); // Referenced entity name
            linkEntityElement.SetAttribute("from", oEntityRelationship.ReferencedAttribute); // Primary key of the referenced entity
            linkEntityElement.SetAttribute("to", oEntityRelationship.ReferencingAttribute); // Foreign key on the referencing entity
            linkEntityElement.SetAttribute("link-type", "inner"); // Type of join
            linkEntityElement.SetAttribute("alias", "linkedEntity");

            // Optionally, you can add attributes to the link-entity
            XmlElement referencedAttributeElement = fetchXmlDoc.CreateElement("attribute");
            referencedAttributeElement.SetAttribute("name", oEntityRelationship.ReferencedAttribute);
            linkEntityElement.AppendChild(referencedAttributeElement);

            // Adding a condition (filter) on the ReferencedAttribute
            if (!string.IsNullOrEmpty(oEntityRelationship.ReferencedAttribute) && !string.IsNullOrEmpty(regardingId))
            {
                // Create <filter> element
                XmlElement filterElement = fetchXmlDoc.CreateElement("filter");
                filterElement.SetAttribute("type", "and");

                // Create <condition> element inside <filter>
                XmlElement conditionElement = fetchXmlDoc.CreateElement("condition");
                conditionElement.SetAttribute("attribute", oEntityRelationship.ReferencedAttribute);
                conditionElement.SetAttribute("operator", "eq"); // You can change the operator based on your logic
                conditionElement.SetAttribute("value", regardingId); // Replace with actual value or pass dynamically

                // Append condition to filter
                filterElement.AppendChild(conditionElement);

                // Append filter to link-entity
                linkEntityElement.AppendChild(filterElement);
            }

            // Append the new <link-entity> to the <entity>
            entityNode.AppendChild(linkEntityElement);

            // Return the modified FetchXML as a string
            return fetchXmlDoc.OuterXml;
        }

        private string AddLinkEntityToFetchXmlForManyToMany(string fetchXml, EntityRelationshipDto oEntityRelationship, GridViewDataReq oGridViewDataReq)
        {
            // Create an XmlDocument and load the existing FetchXML
            XmlDocument fetchXmlDoc = new XmlDocument();
            fetchXmlDoc.LoadXml(fetchXml);

            // Get the root <entity> node where we will append the link-entity
            XmlNode entityNode = fetchXmlDoc.SelectSingleNode("//fetch/entity");

            if (entityNode == null)
            {
                throw new InvalidOperationException("Invalid FetchXML. <entity> node not found.");
            }
            var logicalName = oGridViewDataReq.EntityName==oEntityRelationship.Entity1LogicalName ? oEntityRelationship.Entity2LogicalName : oEntityRelationship.Entity1LogicalName;
            var intersectAttribute1 = oGridViewDataReq.EntityName==oEntityRelationship.Entity1LogicalName? oEntityRelationship.Entity1IntersectAttribute: oEntityRelationship.Entity2IntersectAttribute;
            var intersectAttribute2 = oGridViewDataReq.EntityName==oEntityRelationship.Entity1LogicalName ? oEntityRelationship.Entity2IntersectAttribute : oEntityRelationship.Entity1IntersectAttribute;


            // Create <link-entity> for the intersection entity (the relationship)
            XmlElement linkEntityElement1 = fetchXmlDoc.CreateElement("link-entity");
            linkEntityElement1.SetAttribute("name", oEntityRelationship.IntersectEntityName); // Name of the intersect entity (join table)
            linkEntityElement1.SetAttribute("from", intersectAttribute1); // Intersection attribute from Entity 1
            linkEntityElement1.SetAttribute("to", intersectAttribute1); // Primary key of the referencing entity
            linkEntityElement1.SetAttribute("link-type", "inner"); // Type of join
            linkEntityElement1.SetAttribute("alias", "intersectEntity");

            // Create <link-entity> for Entity2 (target of the relationship)
            XmlElement linkEntityElement2 = fetchXmlDoc.CreateElement("link-entity");
            linkEntityElement2.SetAttribute("name", logicalName); // Target entity name
            linkEntityElement2.SetAttribute("from", intersectAttribute2); // Intersection attribute from Entity 2
            linkEntityElement2.SetAttribute("to", intersectAttribute2); // Target's attribute linking to intersection
            linkEntityElement2.SetAttribute("link-type", "inner"); // Type of join
            linkEntityElement2.SetAttribute("alias", "linkedEntity");

            // Optionally, add RegardingId condition in <filter> for the intersect entity
            if (!string.IsNullOrEmpty(oGridViewDataReq.RegardingId))
            {
                XmlElement filterElement = fetchXmlDoc.CreateElement("filter");
                filterElement.SetAttribute("type", "and");

                XmlElement conditionElement = fetchXmlDoc.CreateElement("condition");
                conditionElement.SetAttribute("attribute", intersectAttribute2);
                conditionElement.SetAttribute("operator", "eq");
                conditionElement.SetAttribute("value", oGridViewDataReq.RegardingId);

                filterElement.AppendChild(conditionElement);

                // Append filter to the intersect entity (link-entity)
                linkEntityElement1.AppendChild(filterElement);
            }

            // Append the second <link-entity> (Entity2) to the first link-entity (intersection entity)
            linkEntityElement1.AppendChild(linkEntityElement2);

            // Append the first <link-entity> (intersection entity) to the <entity> node
            entityNode.AppendChild(linkEntityElement1);

            // Return the modified FetchXML as a string
            return fetchXmlDoc.OuterXml;
        }

        private void ExtractAndApplyConditionsFromFetchXml(string fetchXml, QueryExpression query)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(fetchXml);

            XmlNode filterNode = xmlDoc.SelectSingleNode("//filter");

            if (filterNode != null)
            {
                FilterExpression extractedFilter = new FilterExpression(LogicalOperator.And);

                foreach (XmlNode conditionNode in filterNode.ChildNodes)
                {
                    if (conditionNode.Name == "condition")
                    {
                        string attribute = conditionNode.Attributes["attribute"].Value;
                        string operatorValue = conditionNode.Attributes["operator"].Value;

                        List<object> values = new List<object>();
                        foreach (XmlNode valueNode in conditionNode.ChildNodes)
                        {
                            if (valueNode.Name == "value")
                            {
                                string value = valueNode.InnerText.Trim();
                                values.Add(ParseConditionValue(value));
                            }
                        }

                        ConditionOperator conditionOperator = ParseConditionOperator(operatorValue);
                        ConditionExpression conditionExpression;

                        if (values.Count > 1)
                        {
                            conditionExpression = new ConditionExpression(attribute, conditionOperator, values.ToArray());
                        }
                        else
                        {
                            string singleValue = conditionNode.Attributes["value"]?.Value;
                            object conditionValue = singleValue != null ? ParseConditionValue(singleValue) : values[0];
                            conditionExpression = new ConditionExpression(attribute, conditionOperator, conditionValue);
                        }

                        extractedFilter.Conditions.Add(conditionExpression);
                    }
                }

                query.Criteria.AddFilter(extractedFilter);
            }
        }
        private static ConditionOperator ParseConditionOperator(string operatorValue)
        {
            switch (operatorValue)
            {
                case "eq":
                    return ConditionOperator.Equal;
                case "neq":
                    return ConditionOperator.NotEqual;
                case "gt":
                    return ConditionOperator.GreaterThan;
                case "ge":
                    return ConditionOperator.GreaterEqual;
                case "lt":
                    return ConditionOperator.LessThan;
                case "le":
                    return ConditionOperator.LessEqual;
                case "on-or-after":
                    return ConditionOperator.OnOrAfter;
                case "on-or-before":
                    return ConditionOperator.OnOrBefore;
                case "like":
                    return ConditionOperator.Like;
                case "not-like":
                    return ConditionOperator.NotLike;
                case "in":
                    return ConditionOperator.In;
                case "not-in":
                    return ConditionOperator.NotIn;
                case "between":
                    return ConditionOperator.Between;
                case "not-between":
                    return ConditionOperator.NotBetween;
                case "null":
                    return ConditionOperator.Null;
                case "not-null":
                    return ConditionOperator.NotNull;
                case "begins-with":
                    return ConditionOperator.BeginsWith;
                case "not-begin-with":
                    return ConditionOperator.DoesNotBeginWith;
                case "ends-with":
                    return ConditionOperator.EndsWith;
                case "not-end-with":
                    return ConditionOperator.DoesNotEndWith;
                case "contain-values":
                    return ConditionOperator.ContainValues;
                case "does-not-contain-values":
                    return ConditionOperator.DoesNotContainValues;
                default:
                    throw new ArgumentException("Unsupported operator: " + operatorValue);
            }
        }
        private static object ParseConditionValue(string value)
        {
            if (int.TryParse(value, out int intValue))
            {
                return intValue;
            }
            else if (DateTime.TryParse(value, out DateTime dateTimeValue))
            {
                return dateTimeValue;
            }
            return value;
        }

    }
}
