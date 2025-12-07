using Microsoft.Xrm.Sdk;
using PIF.EBP.Application.MetaData.DTOs;
using PIF.EBP.Application.Shared;
using PIF.EBP.Application.Shared.Helpers;
using System;

namespace PIF.EBP.Application.Accounts.Dtos
{
    public class PortalInvitation
    {
        public Guid Id { get; set; }
        public string InvitationLink { get; set; }
        public DateTime ExpiryDate { get; set; }
        public EntityReferenceDto Company { get; set; }
        public EntityReferenceDto Contact { get; set; }
        public EntityReferenceDto PortalRole { get; set; }
        public EntityOptionSetDto Status { get; set; }

        public static Entity ConvertToCrmEntity(PortalInvitation invitation)
        {
            var invitationEntity = new Entity("hexa_portalinvitation");
            invitationEntity["hexa_portalinvitationid"] = invitation.Id;

            invitationEntity["hexa_invitationlink"] = invitation.InvitationLink;
            invitationEntity["hexa_expirydate"] = invitation.ExpiryDate;

            if(invitation.Company != null)
                invitationEntity["hexa_companyid"] = new EntityReference(EntityNames.PortalInvitation, new Guid(invitation.Company.Id));

            if (invitation.Contact != null)
                invitationEntity["hexa_contactid"] = new EntityReference(EntityNames.PortalInvitation, new Guid(invitation.Contact.Id));

            if (invitation.PortalRole != null)
                invitationEntity["hexa_portalroleid"] = new EntityReference(EntityNames.PortalInvitation, new Guid(invitation.PortalRole.Id));

            if (invitation.Status != null)
            {
                if (int.TryParse(invitation.Status.Value, out int optionSetValue))
                {
                    invitationEntity["hexa_invitationstatustypecode"] = new OptionSetValue(optionSetValue);
                }
                else
                {
                    throw new InvalidCastException("The Value for the status must be a valid integer.");
                }
            }

            return invitationEntity;
        }

        public static PortalInvitation ConvertToModel(Entity invitationEntity)
        {
            return new PortalInvitation()
            {
                Id = CRMOperations.GetValueByAttributeName<Guid>(invitationEntity, "hexa_portalinvitationid"),
                InvitationLink = CRMOperations.GetValueByAttributeName<string>(invitationEntity, "hexa_invitationlink"),
                ExpiryDate = CRMOperations.GetValueByAttributeName<DateTime>(invitationEntity, "hexa_expirydate"),
                Company = CRMOperations.GetValueByAttributeName<EntityReferenceDto>(invitationEntity, "hexa_companyid"),
                Contact = CRMOperations.GetValueByAttributeName<EntityReferenceDto>(invitationEntity, "hexa_contactid"),
                PortalRole = CRMOperations.GetValueByAttributeName<EntityReferenceDto>(invitationEntity, "hexa_portalroleid"),
                Status = CRMOperations.GetValueByAttributeName<EntityOptionSetDto>(invitationEntity, "hexa_invitationstatustypecode")
            };
        }

        public static string[] GetColumns()
        {
            string[] otpColumns = new string[] { "hexa_portalinvitationid", "hexa_invitationlink", "hexa_expirydate", "hexa_companyid", "hexa_contactid", "hexa_portalroleid", "hexa_invitationstatustypecode" };
            return otpColumns;
        }
    }
}
