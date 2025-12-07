using Microsoft.Xrm.Sdk;
using PIF.EBP.Application.MetaData.DTOs;
using PIF.EBP.Application.Shared;
using PIF.EBP.Application.Shared.Helpers;
using System;

namespace PIF.EBP.Application.Accounts.Dtos
{
    public class OtpDto
    {
        public string Name { get; set; }
        public EntityOptionSetDto OperationType { get; set; }
        public EntityReferenceDto Contact { get;  set; }
        public EntityReferenceDto Invitation { get;  set; }
        public DateTime CreatedOn { get; set; }
        public int OTPNumber { get;  set; }

        public static Entity ConvertToCrmEntity(OtpDto otpDto)
        {
            var otpEntity = new Entity(EntityNames.Otp);

            otpEntity["pwc_name"] = otpDto.Name;
            otpEntity["pwc_otpcode"] = otpDto.OTPNumber.ToString();

            if (otpDto.Contact != null)
                otpEntity["pwc_contactid"] = new EntityReference(EntityNames.Contact, new Guid(otpDto.Contact.Id));
            
            if (otpDto.Invitation != null)
                otpEntity["pwc_invitationid"] = new EntityReference(EntityNames.PortalInvitation, new Guid(otpDto.Invitation.Id));

            if (otpDto.OperationType != null)
            {
                if (int.TryParse(otpDto.OperationType.Value, out int optionSetValue))
                {
                    otpEntity["pwc_operationtypetypecode"] = new OptionSetValue(optionSetValue);
                }
                else
                {
                    throw new InvalidCastException("The Value for the operation type must be a valid integer.");
                }
            }
            return otpEntity;
        }

        public static OtpDto ConvertToOtpDto(Entity otpEntity)
        {
            return new OtpDto()
            {
                Name = CRMOperations.GetValueByAttributeName<string>(otpEntity, "pwc_name"),
                OTPNumber = Convert.ToInt32(CRMOperations.GetValueByAttributeName<string>(otpEntity, "pwc_otpcode")),
                Contact = CRMOperations.GetValueByAttributeName<EntityReferenceDto>(otpEntity, "pwc_contactid"),
                Invitation = CRMOperations.GetValueByAttributeName<EntityReferenceDto>(otpEntity, "pwc_invitationid"),
                OperationType = CRMOperations.GetValueByAttributeName<EntityOptionSetDto>(otpEntity, "pwc_operationtypetypecode"),
                CreatedOn = CRMOperations.GetValueByAttributeName<DateTime>(otpEntity, "createdon")
            };
        }

        public static string[] GetOtpColumns()
        {
            string[] otpColumns = new string[] { "pwc_operationtypetypecode", "pwc_contactid", "pwc_invitationid", "pwc_otpcode", "createdon" };
            return otpColumns;
        }
    }
}
