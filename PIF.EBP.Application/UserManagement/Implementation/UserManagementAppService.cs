using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using PIF.EBP.Application.AccessManagement;
using PIF.EBP.Application.EntitiesCache;
using PIF.EBP.Application.MetaData.DTOs;
using PIF.EBP.Application.Shared;
using PIF.EBP.Application.Shared.AppResponse;
using PIF.EBP.Application.Shared.Helpers;
using PIF.EBP.Application.UserManagement.DTOs;
using PIF.EBP.Core.CRM;
using PIF.EBP.Core.Exceptions;
using PIF.EBP.Core.Session;
using PIF.EBP.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static PIF.EBP.Application.Shared.Enums;

namespace PIF.EBP.Application.UserManagement.Implementation
{
    public class UserManagementAppService : IUserManagementAppService
    {
        private readonly ICrmService _crmService;
        private readonly ISessionService _sessionService;
        private readonly IUserPermissionAppService _userPermissionAppService;
        private readonly IAccessManagementAppService _accessMangementService;
        private readonly IEntitiesCacheAppService _entitiesCacheAppService;

        public UserManagementAppService(ICrmService crmService,
            ISessionService sessionService,
            IUserPermissionAppService userPermissionAppService,
            IAccessManagementAppService accessManagementAppService,
            IEntitiesCacheAppService entitiesCacheAppService
            )
        {
            _crmService = crmService;
            _sessionService = sessionService;
            _userPermissionAppService = userPermissionAppService;
            _accessMangementService = accessManagementAppService;
            _entitiesCacheAppService= entitiesCacheAppService;
        }
      
        public async Task<ListPagingResponse<UserListResponse>> RetrieveUsersList(UserListReq oUserListReq)
        {
            ListPagingResponse<UserListResponse> oResponse = new ListPagingResponse<UserListResponse>();
            
            
            var query = await BuildUsersListQueryExpression(oUserListReq);
            
            var entityCollection = _crmService.GetInstance().RetrieveMultiple(query);
            // Get the total count of records
            int totalCount = entityCollection.TotalRecordCount;

            List<UserListResponse> data = MapUsersListToDto(entityCollection);
            oResponse.ListResponse = new List<UserListResponse>();
            oResponse.ListResponse.AddRange(data);
            oResponse.TotalCount = totalCount;
            return oResponse;
        }
      
        public bool DeactivateUser(Guid associationId)
        {
            string[] cols = new string[] { "hexa_contactroleassociationid", "hexa_associationstatustypecode", "hexa_contactid", "hexa_portalroleid" };
            var entity = _crmService.GetById(EntityNames.ContactAssociation, cols, associationId, "hexa_contactroleassociationid");
            Guard.AssertArgumentNotNull(entity);
            var associationStatus = CRMOperations.GetValueByAttributeName<EntityOptionSetDto>(entity, "hexa_associationstatustypecode");
            if (!string.IsNullOrEmpty(associationStatus.Value)&&Convert.ToInt32(associationStatus.Value)==(int)Enums.PortalAssociationStatus.Active)
            {
                var updateEntity = new Entity(EntityNames.ContactAssociation);
                updateEntity["hexa_contactroleassociationid"] = associationId;
                updateEntity["hexa_associationstatustypecode"] = new OptionSetValue((int)PortalAssociationStatus.Inactive);
                _crmService.Update(updateEntity, EntityNames.ContactAssociation);
                return true;
            }
            return false;
        }
      
        public UserInviteRes ReinviteUser(UserInviteReq userInviteReqlist)
        {
            var deletedAssociation = _accessMangementService.GetDeletedConactRoleAssociations(userInviteReqlist.ContactId.ToString(), _sessionService.GetCompanyId()).FirstOrDefault();

            if(deletedAssociation == null)
            {
                throw new UserFriendlyException("UserHasNoDeletedAssociationsWithThisCompany");
            }
            try
            {
                var updatedAssociation = new Entity(EntityNames.ContactAssociation);
                updatedAssociation.Id = new Guid(deletedAssociation.Id);
                updatedAssociation["hexa_associationstatustypecode"] = new OptionSetValue((int)PortalAssociationStatus.Active);
                _crmService.Update(updatedAssociation, EntityNames.ContactAssociation);
                return new UserInviteRes { IsSuccess=true, ContactId= userInviteReqlist.ContactId, Message="UserReinvitedSuccessfully" };
            }catch(Exception)
            {
                throw new UserFriendlyException("MsgUnexpectedError");
            }
        }
     
        public string ResendInviteUser(UserReSendReq oUserReSendReq)
        {
            var associationCols = new ColumnSet("hexa_contactroleassociationid", "hexa_contactid", "hexa_portalroleid", "hexa_associationstatustypecode", "hexa_companyid");
            QueryExpression query = new QueryExpression(EntityNames.ContactAssociation)
            {
                ColumnSet = associationCols,
            };
            var rolelink = new LinkEntity
            {
                LinkFromEntityName = EntityNames.ContactAssociation,
                LinkFromAttributeName = "hexa_portalroleid",
                LinkToEntityName = EntityNames.PortalRole,
                LinkToAttributeName = "hexa_portalroleid",
                JoinOperator = JoinOperator.Inner,
                Columns= new ColumnSet("hexa_portalroleid", "pwc_departmentid", "pwc_parentportalroleid"),
                EntityAlias="Role"
            };
            query.LinkEntities.Add(rolelink);
            query.Criteria.AddCondition("hexa_contactroleassociationid", ConditionOperator.Equal, oUserReSendReq.AssociationId);
            var entity = _crmService.GetInstance().RetrieveMultiple(query).Entities.FirstOrDefault();
            Guard.AssertArgumentNotNull(entity);
            var associationStatus = CRMOperations.GetValueByAttributeName<EntityOptionSetDto>(entity, "hexa_associationstatustypecode");
            if (!string.IsNullOrEmpty(associationStatus.Value)&&Convert.ToInt32(associationStatus.Value)==(int)Enums.PortalAssociationStatus.Invited)
            {
                OrganizationRequest ResendInvite = new OrganizationRequest("pwc_EBPResendInvitationAction");
                ResendInvite["ContactGuid"] = CRMOperations.GetValueByAttributeName<EntityReferenceDto>(entity, "hexa_contactid").Id;
                ResendInvite["CompanyGuid"] = CRMOperations.GetValueByAttributeName<EntityReferenceDto>(entity, "hexa_companyid").Id;
                ResendInvite["PortalRoleGuid"] = CRMOperations.GetValueByAttributeName<EntityReferenceDto>(entity, "hexa_portalroleid").Id;
                ResendInvite["ExternalDepartmentGuid"]="";

                var orgService = _crmService.GetInstance();
                var orgResponse = orgService.Execute(ResendInvite);
                return orgResponse.Results.Values.FirstOrDefault().ToString();
            }
            else
                throw new UserFriendlyException("UserShouldInvited");

        }

        private async Task<QueryExpression> BuildUsersListQueryExpression(UserListReq oUserListReq)
        {
            var opagingInfo = CRMOperations.GetPagingInfo(oUserListReq.PagingRequest);
            var companyId = _sessionService.GetCompanyId();

            var associationCols = new ColumnSet("hexa_contactroleassociationid", "hexa_contactid", "hexa_portalroleid", "hexa_associationstatustypecode", "hexa_companyid");

            QueryExpression query = new QueryExpression(EntityNames.ContactAssociation)
            {
                ColumnSet = associationCols,
                PageInfo = opagingInfo
            };

            // --- Contact Link ---
            var contactlink = new LinkEntity
            {
                LinkFromEntityName = EntityNames.ContactAssociation,
                LinkFromAttributeName = "hexa_contactid",
                LinkToEntityName = EntityNames.Contact,
                LinkToAttributeName = "contactid",
                JoinOperator = JoinOperator.Inner,
                Columns = new ColumnSet("contactid", "firstname", "lastname", "ntw_firstnamearabic", "ntw_lastnamearabic", "emailaddress1", "pwc_position", "entityimage"),
                EntityAlias = "Contact",

                // --- Nested Membership & MembershipType ---
                LinkEntities =
    {
        new LinkEntity
        {
            LinkFromEntityName = EntityNames.Contact,
            LinkFromAttributeName = "contactid",
            LinkToEntityName = EntityNames.Membership,
            LinkToAttributeName = "ntw_contactname",
            JoinOperator = JoinOperator.LeftOuter,
            Columns = new ColumnSet("ntw_membershipid", "ntw_companyname", "ntw_memberinset"),
            EntityAlias = "Membership",
            LinkCriteria = new FilterExpression(LogicalOperator.And)
        {
            Conditions =
            {
                new ConditionExpression("ntw_companyname", ConditionOperator.Equal, new Guid(companyId))
            }
        }
        }
                }
                };
            new LinkEntity
            {
                LinkFromEntityName = EntityNames.Contact,
                LinkFromAttributeName = "pwc_position",
                LinkToEntityName = EntityNames.Position,
                LinkToAttributeName = "ntw_positionid",
                JoinOperator = JoinOperator.LeftOuter,
                Columns = new ColumnSet("ntw_positionid", "ntw_name", "ntw_namear"),
                EntityAlias = "Position"
            };

            // --- Role Link ---
            var rolelink = new LinkEntity
            {
                LinkFromEntityName = EntityNames.ContactAssociation,
                LinkFromAttributeName = "hexa_portalroleid",
                LinkToEntityName = EntityNames.PortalRole,
                LinkToAttributeName = "hexa_portalroleid",
                JoinOperator = JoinOperator.Inner,
                Columns = new ColumnSet("hexa_portalroleid", "pwc_departmentid", "pwc_parentportalroleid", "hexa_name", "pwc_namear", "pwc_roletypetypecode"),
                EntityAlias = "Role",
                LinkEntities =
                          {
                               new LinkEntity
                                     {
                                              LinkFromEntityName = EntityNames.PortalRole,
                                              LinkFromAttributeName = "pwc_departmentid",
                                              LinkToEntityName = EntityNames.ExternalDepartment,
                                              LinkToAttributeName = "pwc_externaldepartmentid",
                                              JoinOperator = JoinOperator.LeftOuter,
                                              Columns = new ColumnSet("pwc_externaldepartmentid", "pwc_name", "pwc_nameexternaldepartment"),
                                              EntityAlias = "Department"
                                     }
                                }
            };

            var roleAssociationFilter = new FilterExpression();
            roleAssociationFilter.AddCondition("Role", "pwc_roletypetypecode", ConditionOperator.Equal, (int)RoleType.NonBoardMember);
            query.Criteria.AddFilter(roleAssociationFilter);

            query.Criteria.AddCondition("Membership", "ntw_membershipid", ConditionOperator.Null);
            // --- Query Conditions ---
            query.Criteria.AddCondition(new ConditionExpression("hexa_companyid", ConditionOperator.Equal, new Guid(companyId)));
            query.Criteria.AddCondition(new ConditionExpression("hexa_associationstatustypecode", ConditionOperator.NotEqual, (int)PortalAssociationStatus.Deleted));

            // --- Search Term Filter (on Contact fields) ---
            if (!string.IsNullOrEmpty(oUserListReq.SearchTerm))
            {
                var includeEFilter = new FilterExpression(LogicalOperator.Or);
                includeEFilter.AddCondition(new ConditionExpression("firstname", ConditionOperator.Like, $"%{oUserListReq.SearchTerm}%"));
                includeEFilter.AddCondition(new ConditionExpression("lastname", ConditionOperator.Like, $"%{oUserListReq.SearchTerm}%"));
                includeEFilter.AddCondition(new ConditionExpression("ntw_firstnamearabic", ConditionOperator.Like, $"%{oUserListReq.SearchTerm}%"));
                includeEFilter.AddCondition(new ConditionExpression("ntw_lastnamearabic", ConditionOperator.Like, $"%{oUserListReq.SearchTerm}%"));
                includeEFilter.AddCondition(new ConditionExpression("emailaddress1", ConditionOperator.Like, $"%{oUserListReq.SearchTerm}%"));

                contactlink.LinkCriteria = includeEFilter;
            }

            // --- Role Filter ---
            if (oUserListReq.RoleFilter != Guid.Empty)
            {
                var includeRoleFilter = new FilterExpression(LogicalOperator.Or);
                includeRoleFilter.AddCondition(new ConditionExpression("hexa_portalroleid", ConditionOperator.Equal, oUserListReq.RoleFilter));
                includeRoleFilter.AddCondition(new ConditionExpression("pwc_parentportalroleid", ConditionOperator.Equal, oUserListReq.RoleFilter));
                rolelink.LinkCriteria = includeRoleFilter;
            }

            // --- Reassign Logic ---
            if (oUserListReq.IsReassignOperation)
            {
                var IsBoardMember = await _userPermissionAppService.IsLoggedInUserIsBoardMember();
                var viewerRoleIds = await _userPermissionAppService.GetViewerRoles();
                var boardMemberRoleIds = await _userPermissionAppService.GetBoardMemberRoles();
                var combinedRoleIds = viewerRoleIds.Concat(boardMemberRoleIds).ToList();

                if (IsBoardMember && boardMemberRoleIds.Count > 0)
                {
                    query.Criteria.AddCondition(new ConditionExpression("hexa_portalroleid", ConditionOperator.In, boardMemberRoleIds.ToArray()));
                }
                else if (combinedRoleIds.Count > 0)
                {
                    query.Criteria.AddCondition(new ConditionExpression("hexa_portalroleid", ConditionOperator.NotIn, combinedRoleIds.ToArray()));
                }

                query.Criteria.AddCondition(new ConditionExpression("hexa_associationstatustypecode", ConditionOperator.Equal, (int)PortalAssociationStatus.Active));
            }

            // --- Sorting ---
            ApplySorting(oUserListReq, query, contactlink);

            // --- Add Link Entities ---
            query.LinkEntities.Add(contactlink);
            query.LinkEntities.Add(rolelink);

            return query;

        }

        private void ApplySorting(UserListReq oUserListReq, QueryExpression query, LinkEntity contactlink)
        {
            if (!string.IsNullOrEmpty(oUserListReq.PagingRequest.SortField))
            {
                switch (oUserListReq.PagingRequest.SortField.ToLower())
                {
                    case "firstname":
                        oUserListReq.PagingRequest.SortField="firstname";
                        CRMOperations.AddOrderToLinkEntity(contactlink, oUserListReq.PagingRequest);
                        break;
                    case "lastname":
                        oUserListReq.PagingRequest.SortField="lastname";
                        CRMOperations.AddOrderToLinkEntity(contactlink, oUserListReq.PagingRequest);
                        break;
                    case "email":
                        oUserListReq.PagingRequest.SortField="emailaddress1";
                        CRMOperations.AddOrderToLinkEntity(contactlink, oUserListReq.PagingRequest);
                        break;
                    case "role":
                        oUserListReq.PagingRequest.SortField="hexa_portalroleid";
                        query.AddOrder(oUserListReq.PagingRequest.SortField, (OrderType)oUserListReq.PagingRequest.SortOrder);
                        break;
                    case "associationstatus":
                        oUserListReq.PagingRequest.SortField="hexa_associationstatustypecode";
                        query.AddOrder(oUserListReq.PagingRequest.SortField, (OrderType)oUserListReq.PagingRequest.SortOrder);
                        break;
                    default:
                        break;
                }
            }
        }

        private List<UserListResponse> MapUsersListToDto(EntityCollection entityCollection)
        {
            return entityCollection.Entities.Select(entity => FillUsersList(entity)).ToList();
        }

        private UserListResponse FillUsersList(Entity entity)
        {
            UserListResponse oUserListResponse = new UserListResponse();
            var position = entity.Contains("Contact.pwc_position") ? ((EntityReference)CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "Contact.pwc_position").Value) : null;
            var PositionAr = entity.Contains("Position.ntw_namear") ? ((AliasedValue)entity.Attributes["Position.ntw_namear"]).Value.ToString() : string.Empty;

            var department = entity.Contains("Role.pwc_departmentid") ? ((EntityReference)CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "Role.pwc_departmentid").Value) : null;
            var departmentAr = entity.Contains("Department.pwc_nameexternaldepartment") ? ((AliasedValue)entity.Attributes["Department.pwc_nameexternaldepartment"]).Value.ToString() : string.Empty;

            var userImage = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "Contact.entityimage")?.Value;

            oUserListResponse.Id=entity.Id;
            oUserListResponse.ContactId=CRMOperations.GetValueByAttributeName<EntityReferenceDto>(entity, "hexa_contactid").Id;
            oUserListResponse.FirstName = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "Contact.firstname")?.Value.ToString() ??string.Empty;
            oUserListResponse.LastName = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "Contact.lastname")?.Value.ToString()??string.Empty;
            oUserListResponse.FirstNameAr = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "Contact.ntw_firstnamearabic")?.Value.ToString()??string.Empty;
            oUserListResponse.LastNameAr = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "Contact.ntw_lastnamearabic")?.Value.ToString()??string.Empty;
            oUserListResponse. Email = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "Contact.emailaddress1")?.Value?.ToString();
            oUserListResponse.Entityimage =userImage ==null ? new byte[0] : (byte[])userImage;
            oUserListResponse.Company =CRMOperations.GetValueByAttributeName<EntityReferenceDto>(entity, "hexa_companyid");
            oUserListResponse.AssociationStatus=_entitiesCacheAppService.RetrieveOptionSetCacheByKeyWithValue(OptionSetKey.Associationtatus, CRMOperations.GetValueByAttributeName<OptionSetValue>(entity, "hexa_associationstatustypecode"));
            oUserListResponse.Department=department ==null ? null : new EntityReferenceDto { Id=department.Id.ToString(), Name=department.Name, NameAr=departmentAr };
            oUserListResponse.Position=position ==null ? null : new EntityReferenceDto { Id=position.Id.ToString(), Name=position.Name, NameAr=PositionAr };
            oUserListResponse.Role = CRMOperations.GetValueByAttributeName<EntityReferenceDto>(entity, "hexa_portalroleid", "Role.pwc_namear");
            if (!string.IsNullOrEmpty(oUserListResponse.AssociationStatus.Value))
            {
                var status = Convert.ToInt32(oUserListResponse.AssociationStatus.Value);
                ApplyRoleCheckFlags(oUserListResponse, status);
            }
            return oUserListResponse;
        }

        private void ApplyRoleCheckFlags(UserListResponse oUserListResponse, int status)
        {
            var isAdmin = _userPermissionAppService.IsLoggedInUserIsAdmin();
            if (isAdmin)
            {
                if (oUserListResponse.Role !=null)
                {
                    var ispermission = _userPermissionAppService.CheckReadUserPermissions(new Guid(oUserListResponse.ContactId), new Guid(oUserListResponse.Role.Id));
                    if (ispermission)
                    {

                        if (status == (int)Enums.PortalAssociationStatus.Invited)
                            oUserListResponse.Resend=true;
                        else if (status == (int)Enums.PortalAssociationStatus.Active)
                        {
                            oUserListResponse.Deactivate=true;
                            oUserListResponse.Editable = true;
                        }
                        else if (status==(int)Enums.PortalAssociationStatus.Inactive)
                            oUserListResponse.ReInvite=true;
                    }
                }
            }
            else
            {
                oUserListResponse.Resend = oUserListResponse.Deactivate = oUserListResponse.Editable = oUserListResponse.ReInvite = false;
            }
        }
    }
}
