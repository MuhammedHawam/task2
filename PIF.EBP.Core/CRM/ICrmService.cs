using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using PIF.EBP.Core.DependencyInjection;
using System;
using System.Xml.Linq;

namespace PIF.EBP.Core.CRM
{
    public interface ICrmService : IScopedDependency
    {
        EntityMetadata GetMetaData(string entityName);
        IOrganizationService GetInstance();
        Guid Create(Entity entity, string entityName);
        EntityCollection GetAll(string entityName, string[] columns, object columnValue = null, string columnName = null);
        Entity GetById(string entityName, string[] columns, Guid? columnValue = null, string columnName = null);
        void Update(Entity entity, string entityName);
        void Delete(string id, string entityName);
        EntityCollection RetrieveAllRecordsUsingFetchXml(string fetchXml);
        EntityCollection RetrievePagingRecordsUsingFetchXml(string fetchXml, int pageNumber, int pageSize,string sortField,int sortOrder);
        void ExecuteWorkflow(IOrganizationService organizationService, Guid entityID, Guid workflowId);
        XDocument GetFetchXml(QueryExpression query);
    }
}
