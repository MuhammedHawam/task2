using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using PIF.EBP.Application.Shared.Helpers;
using PIF.EBP.Core.CRM;
using PIF.EBP.Core.Exceptions;
using PIF.EBP.Core.FileManagement.Consts;
using System;
using System.Linq;

namespace PIF.EBP.Application.DocumentLocation.Implementation
{
    public class DocumentLocationAppService : IDocumentLocationAppService
    {
        private readonly ICrmService _crmService;
        public DocumentLocationAppService(ICrmService crmService)
        {
            _crmService = crmService;
        }

        public void CreateSharePointDocumentLocation(string entityLogicalName,EntityReference oEntityReference)
        {
            string folderName = string.Empty;
            folderName = GetFormattedFolderName(oEntityReference.Id.ToString(), string.Empty);

            EntityReference entRefDocLocation = GetDefaultDocLocation(entityLogicalName);

            Entity oEntity = new Entity(SharePointDocumentLocation.EntityLogicalName);
            oEntity.Attributes[SharePointDocumentLocation.Name] = folderName;
            oEntity.Attributes[SharePointDocumentLocation.ParentSiteOrLocation] = entRefDocLocation;
            oEntity.Attributes[SharePointDocumentLocation.RelativeUrl] = folderName;
            oEntity.Attributes[SharePointDocumentLocation.RegardingObjectId] = oEntityReference;
            _crmService.GetInstance().Create(oEntity);
        }
        private EntityReference GetDefaultDocLocation(string entityLogicalName)
        {
            EntityReference entrefSpLocation = null;

            ColumnSet columns = new ColumnSet
                (
                    SharePointDocumentLocation.Id,
                    SharePointDocumentLocation.RelativeUrl
                );
            QueryExpression query = new QueryExpression(SharePointDocumentLocation.EntityLogicalName)
            {
                ColumnSet = columns,
                Criteria = new FilterExpression
                {
                    Conditions =
                            {
                                new ConditionExpression(SharePointDocumentLocation.RelativeUrl, ConditionOperator.Equal, entityLogicalName),
                                new ConditionExpression(SharePointDocumentLocation.StateCode, ConditionOperator.Equal, 0)
                            }
                },
                Orders =
                            {
                                new OrderExpression(SharePointDocumentLocation.Createdon, OrderType.Descending)
                            },
                TopCount = 1
            };

            var entityCollection = _crmService.GetInstance().RetrieveMultiple(query);

            if (entityCollection != null && entityCollection.Entities.Count > 0)
            {
                var entity = entityCollection.Entities.FirstOrDefault();
                if (entity != null)
                    entrefSpLocation = entity.ToEntityReference();
            }
            else
            {
                throw new UserFriendlyException("RootFolderNotFound");
            }
            return entrefSpLocation;
        }
        public string GetDocLocationByRegardingId(Guid regardingId)
        {
            string relativeUrl = string.Empty;

            ColumnSet columns = new ColumnSet
                (
                    SharePointDocumentLocation.Id,
                    SharePointDocumentLocation.RelativeUrl,
                    SharePointDocumentLocation.RegardingObjectId
                );
            QueryExpression query = new QueryExpression(SharePointDocumentLocation.EntityLogicalName)
            {
                ColumnSet = columns,
                Criteria = new FilterExpression
                {
                    Conditions =
                            {
                                new ConditionExpression(SharePointDocumentLocation.RegardingObjectId, ConditionOperator.Equal, regardingId),
                                new ConditionExpression(SharePointDocumentLocation.StateCode, ConditionOperator.Equal, 0)
                            }
                },
                Orders =
                            {
                                new OrderExpression(SharePointDocumentLocation.Createdon, OrderType.Descending)
                            },
                TopCount = 1
            };

            var entityCollection = _crmService.GetInstance().RetrieveMultiple(query);

            if (entityCollection != null && entityCollection.Entities.Count > 0)
            {
                var entity = entityCollection.Entities.FirstOrDefault();
                if (entity != null)
                    relativeUrl = entity.GetValueByAttributeName<string>(SharePointDocumentLocation.RelativeUrl);
            }
            return relativeUrl;
        }

        private string GetFormattedFolderName(string recordId, string recordName)
        {
            string strFormattedFolderName;

            try
            {
                string guidString = recordId != string.Empty
                    ? recordId.ToString().Replace("-", string.Empty).ToUpper()
                    : string.Empty.ToString();

                strFormattedFolderName = recordName + "_" + guidString;

            }
            catch (Exception)
            {
                throw;
            }

            return strFormattedFolderName;
        }
    }
}
