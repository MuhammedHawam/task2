using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Extensions;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using PIF.EBP.Core.FileManagement.DTOs;
using System;
using System.Linq;
using System.Security;
using System.Xml;
using System.Xml.Linq;

namespace PIF.EBP.Core.CRM.Implementation
{
    public class CrmService : ICrmService
    {
        private readonly IOrganizationService _orgService;
        public CrmService(IOrganizationService organizationService)
        {
            _orgService = organizationService;
        }

        public EntityMetadata GetMetaData(string entityName)
        {
            var result = _orgService.GetEntityMetadata(entityName);

            return result;
        }

        public IOrganizationService GetInstance()
        {
            return _orgService ?? throw new NullReferenceException("Org Service is not instanciated");
        }

        public Guid Create(Entity entity, string entityName)
        {
            entity.LogicalName = entityName;
            return _orgService.Create(entity);
        }

        public EntityCollection GetAll(string entityName, string[] columns, object columnValue = null, string columnName = null)
        {
            QueryExpression query = new QueryExpression(entityName)
            {
                ColumnSet = new ColumnSet(columns)
            };

            if (columnValue != null)
            {
                query.Criteria.AddCondition(columnName, ConditionOperator.Equal, columnValue);
            }

            EntityCollection results = _orgService.RetrieveMultiple(query);
            return results;
        }

        public Entity GetById(string entityName, string[] columns, Guid? columnValue = null, string columnName = null)
        {
            QueryExpression query = new QueryExpression(entityName)
            {
                ColumnSet = new ColumnSet(columns)
            };

            if (columnValue != null)
            {
                query.Criteria.AddCondition(columnName, ConditionOperator.Equal, columnValue);
            }

            EntityCollection results = _orgService.RetrieveMultiple(query);
            return results.Entities.Any() ? results.Entities[0] : null;
        }

        public void Update(Entity entity, string entityName)
        {
            _orgService.Update(entity);
        }

        public void Delete(string id, string entityName)
        {
            var guidContactId = new Guid(id);
            _orgService.Delete(entityName, guidContactId);
        }

        public EntityCollection RetrieveAllRecordsUsingFetchXml(string fetchXml)
        {
            EntityCollection entcolTotalRcrds = new EntityCollection();

            try
            {
                bool moreRecords = false;
                int page = 1;
                string cookie = string.Empty;

                do
                {
                    string xml = string.Format(fetchXml, cookie);
                    EntityCollection entcolLooped = _orgService.RetrieveMultiple(new FetchExpression(xml));

                    if (entcolLooped != null && entcolLooped.Entities != null && entcolLooped.Entities.Count > 0)
                    {
                        entcolTotalRcrds.Entities.AddRange(entcolLooped.Entities);

                        moreRecords = entcolLooped.MoreRecords;
                        if (moreRecords)
                        {
                            page++;
                            cookie = string.Format("paging-cookie='{0}' page='{1}'", SecurityElement.Escape(entcolLooped.PagingCookie), page);
                        }
                    }
                }
                while (moreRecords);
            }
            catch (Exception)
            {
                //ignored
            }
            return entcolTotalRcrds;
        }

        public EntityCollection RetrievePagingRecordsUsingFetchXml(string fetchXml, int pageNumber, int pageSize, string sortField, int sortOrder)
        {
            XmlDocument fetchXmlDoc = new XmlDocument();
            fetchXmlDoc.LoadXml(fetchXml);

            XmlNode mainEntity = fetchXmlDoc.SelectSingleNode("//entity");
            AddStateCodeConditionIfMissing(mainEntity);

            if (!string.IsNullOrEmpty(sortField))
            {
                // Remove existing order nodes
                XmlNodeList existingOrderNodes = mainEntity.SelectNodes("order");
                foreach (XmlNode node in existingOrderNodes)
                {
                    mainEntity.RemoveChild(node);
                }

                // Add new order node
                XmlNode orderNode = fetchXmlDoc.CreateElement("order");

                XmlAttribute attributeName = fetchXmlDoc.CreateAttribute("attribute");
                attributeName.Value = sortField;
                orderNode.Attributes.Append(attributeName);

                XmlAttribute descending = fetchXmlDoc.CreateAttribute("descending");
                descending.Value = sortOrder == (int)SortOrder.Descending ? "true" : "false";
                orderNode.Attributes.Append(descending);

                mainEntity.AppendChild(orderNode);
            }

            fetchXml = fetchXmlDoc.OuterXml;

            if (pageNumber > 0 && pageSize > 0)
            {
                // Add paging to fetchxml
                var fetchXmlWithPaging = AddPagingToFetchXml(fetchXml, pageNumber, pageSize);
                EntityCollection results = _orgService.RetrieveMultiple(new FetchExpression(fetchXmlWithPaging));
                return results;
            }
            else
            {
                EntityCollection results = _orgService.RetrieveMultiple(new FetchExpression(fetchXml));
                return results;
            }
        }

        public void ExecuteWorkflow(IOrganizationService organizationService, Guid entityID, Guid workflowId)
        {
            try
            {
                ExecuteWorkflowRequest workflowRequest = new ExecuteWorkflowRequest()
                {
                    WorkflowId = workflowId,
                    EntityId = entityID,
                };
                organizationService.Execute(workflowRequest);
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(ex.ToString());
            }
        }

        public XDocument GetFetchXml(QueryExpression query)
        {
            var request = new QueryExpressionToFetchXmlRequest { Query = query };
            var response = (QueryExpressionToFetchXmlResponse)_orgService.Execute(request);
            return XDocument.Parse(response.FetchXml);
        }

        private void AddStateCodeConditionIfMissing(XmlNode entityNode)
        {
            // Check if this entity (or link-entity) already has a statecode condition
            bool hasStateCodeCondition = entityNode.SelectSingleNode("filter/condition[@attribute='statecode']") != null;

            // If statecode condition is missing, add it
            if (!hasStateCodeCondition)
            {
                // Find the filter node or create one if it doesn't exist
                XmlNode filterNode = entityNode.SelectSingleNode("filter");
                if (filterNode == null)
                {
                    // Create a new filter node
                    filterNode = entityNode.OwnerDocument.CreateElement("filter");
                    ((XmlElement)filterNode).SetAttribute("type", "and");
                    entityNode.AppendChild(filterNode);
                }

                // Create the statecode condition node
                XmlDocument doc = entityNode.OwnerDocument;
                XmlElement conditionElement = doc.CreateElement("condition");
                conditionElement.SetAttribute("attribute", "statecode");
                conditionElement.SetAttribute("operator", "eq");
                conditionElement.SetAttribute("value", "0");

                // Add the condition to the filter node
                filterNode.AppendChild(conditionElement);
            }
        }

        private string AddPagingToFetchXml(string fetchXml, int pageNumber, int pageSize)
        {
            // Add the paging attributes to the fetchxml
            var doc = new XmlDocument();
            doc.LoadXml(fetchXml);
            var fetchNode = doc.SelectSingleNode("/fetch");
            if (fetchNode != null)
            {
                var totalrecordAttr = doc.CreateAttribute("returntotalrecordcount");
                totalrecordAttr.Value ="true";
                fetchNode.Attributes.Append(totalrecordAttr);

                var pageAttr = doc.CreateAttribute("page");
                pageAttr.Value = pageNumber.ToString();
                fetchNode.Attributes.Append(pageAttr);

                var countAttr = doc.CreateAttribute("count");
                countAttr.Value = pageSize.ToString();
                fetchNode.Attributes.Append(countAttr);


            }
            return doc.OuterXml;
        }
    }
}
