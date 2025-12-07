using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using PIF.EBP.Application.MetaData.DTOs;
using PIF.EBP.Application.Shared.AppRequest;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace PIF.EBP.Application.Shared.Helpers
{
    public static class CRMOperations
    {
        public static T GetValueByAttributeName<T>(this Entity entity, string attributeName, string attributeNameAr="")
        {
            if (typeof(T) == typeof(EntityOptionSetDto))
            {
                var optionSetName = string.Empty;
                int? optionSetValue = null;

                if (entity.Contains(attributeName) && entity.Attributes[attributeName] is OptionSetValue optionSet)
                {
                    optionSetValue = optionSet.Value;
                    optionSetName = entity.FormattedValues.ContainsKey(attributeName) ? entity.FormattedValues[attributeName] : string.Empty;
                }

                var entityOptionSetDto = new EntityOptionSetDto
                {
                    Value = optionSetValue?.ToString() ?? string.Empty,
                    Name = optionSetName
                };

                return (T)(object)entityOptionSetDto;
            }

            if (typeof(T) == typeof(EntityReferenceDto))
            {
                var entityReference = entity.GetAttributeValue<EntityReference>(attributeName);
                if (entityReference == null) return default;
                
                var entityReferenceDto = new EntityReferenceDto
                {
                    Id = entityReference.Id.ToString(),
                    Name = entityReference.Name
                };
                if (!string.IsNullOrEmpty(attributeNameAr))
                {
                    entityReferenceDto.NameAr=entity.Contains(attributeNameAr) ? ((AliasedValue)entity.Attributes[attributeNameAr]).Value.ToString() : string.Empty;
                }

                return (T)(object)entityReferenceDto;
            }

            if (typeof(T) == typeof(List<EntityOptionSetDto>))
            {
                var multiSelectValues = new List<EntityOptionSetDto>();
                if (entity.Contains(attributeName) && entity.Attributes[attributeName] is OptionSetValueCollection optionSetCollection)
                {
                    foreach (var optionSet in optionSetCollection)
                    {
                        var optionSetName = entity.FormattedValues.ContainsKey(attributeName)
                                             ? entity.FormattedValues[attributeName]
                                             : string.Empty;

                        multiSelectValues.Add(new EntityOptionSetDto
                        {
                            Value = optionSet.Value.ToString(),
                            Name = optionSetName
                        });
                    }
                }

                return (T)(object)multiSelectValues;
            }
            // Default case for retrieving simple attribute values
            return entity.Contains(attributeName) ? (T)entity.Attributes[attributeName] : default;
        }
        public static string GetValueByAttrNameAlised(Entity entity, string attrName)
        {
            return entity.Contains(attrName) ? ((EntityReference)((AliasedValue)entity.Attributes[attrName]).Value).Id.ToString() : string.Empty;
        }
        public static string GetNameValueByAttrNameAlised(Entity entity, string attrName)
        {
            return entity.Contains(attrName) ? ((EntityReference)((AliasedValue)entity.Attributes[attrName]).Value).Name.ToString() : string.Empty;
        }
        public static int? GetOptionSetValue(Entity entity, string attributeName)
        {
            if (entity.Attributes.TryGetValue(attributeName, out var attributeValue))
            {
                if (attributeValue is OptionSetValue optionSetValue)
                {
                    return optionSetValue.Value;
                }
            }
            return null;
        }
        public static PagingInfo GetPagingInfo(PagingRequest oPagingRequest)
        {
            PagingInfo opagingInfo;
            if (oPagingRequest != null)
                opagingInfo = new PagingInfo { PageNumber = oPagingRequest.PageNo, Count = oPagingRequest.PageSize, ReturnTotalRecordCount = true };
            else
                opagingInfo = new PagingInfo { PageNumber = 1, Count = 10, ReturnTotalRecordCount = true };
            return opagingInfo;
        }
        public static void GetQueryExpression(QueryExpression query, PagingRequest oPagingRequest, Dictionary<string, string> fieldsMappDic)
        {
            if (oPagingRequest != null && !string.IsNullOrEmpty(oPagingRequest.SortField))
            {
                query.AddOrder(fieldsMappDic.FirstOrDefault(x => x.Value.ToLower() == oPagingRequest.SortField.ToLower()).Key.ToString(), (OrderType)oPagingRequest.SortOrder);
            }
        }
        public static void AddOrderToLinkEntity(LinkEntity linkEntity, PagingRequest oPagingRequest)
        {
            var order = new OrderExpression
            {
                AttributeName =oPagingRequest.SortField,
                OrderType =(OrderType)oPagingRequest.SortOrder
            };
            linkEntity.Orders.Add(order);
        }
        public static FilterExpression GetFilterExpression(List<FieldsFilters> filters, Dictionary<string, string> fieldsMappDic)
        {
            var filterExpression = new FilterExpression();
            foreach (var item in filters)
            {
                AddToFilter(fieldsMappDic, filterExpression, item);
            }
            return filterExpression;
        }

        public static void MakeAggregate(XDocument fetchXmlDoc)
        {
            fetchXmlDoc.Root.SetAttributeValue("aggregate", "true");
            fetchXmlDoc.Descendants("attribute").Remove();
        }
        public static void AddAggregateColumn(XDocument fetchXmlDoc, string entity, string attribute, string aggregate, string alias = "")
        {
            var entityElement = FindEntity(ref fetchXmlDoc, entity);
            if (entityElement == null)
                return;
            var attributeAlias = attribute;
            if (alias != "")
                attributeAlias = alias;
            else
                if (entityElement.Name == "link-entity")
                attributeAlias = entity + "." + attribute;
            var attributes = new object[]
            {
                new XAttribute("name", attribute), new XAttribute("alias", attributeAlias), new XAttribute("aggregate", aggregate)
            };

            entityElement.Add(new XElement("attribute", attributes));
        }
        public static FetchExpression GetFetchExpression(XDocument fetchXmlDoc)
        {
            return new FetchExpression(fetchXmlDoc.ToString());
        }
       
        public static T GetAliasedField<T>(Entity data, string key)
        {
            var ret = data.Attributes.ContainsKey(key) ? data.GetAttributeValue<AliasedValue>(key).Value : default(T);
            if (ret == null)
                return default(T);
            return (T)ret;
        }
        private static XElement FindEntity(ref XDocument fetchXmlDoc, string entity)
        {
            var entityElement = fetchXmlDoc.Descendants().Where(x => (x.Name == "entity" || x.Name == "link-entity") && x.Attributes("name").FirstOrDefault().Value == entity).FirstOrDefault();
            var entityName = entityElement.Attributes("name").FirstOrDefault().Value;
            if (entityName == entity)
            {
                return entityElement;
            }
            return null;
        }
        private static void AddToFilter(Dictionary<string, string> fieldsMappDic, FilterExpression filterExpression, FieldsFilters item)
        {
            if (fieldsMappDic.Any(x => x.Value == item.FieldName))
            {
                ConditionExpression conditionExpression = new ConditionExpression();
                conditionExpression.AttributeName = fieldsMappDic.FirstOrDefault(x => x.Value == item.FieldName).Key.ToString();
                conditionExpression.Operator = (ConditionOperator)item.MatchMode;

                if (conditionExpression.Operator == ConditionOperator.Like || conditionExpression.Operator == ConditionOperator.NotLike)
                    conditionExpression.Values.Add("%" + item.Value.ToLower() + "%");
                else
                    conditionExpression.Values.Add(item.Value.ToLower());
                filterExpression.AddCondition(conditionExpression);
            }
        }
    }
}


