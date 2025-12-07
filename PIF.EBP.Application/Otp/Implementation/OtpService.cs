using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk;
using PIF.EBP.Application.Accounts.Dtos;
using PIF.EBP.Application.Shared.Helpers;
using PIF.EBP.Application.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PIF.EBP.Application.Shared.Enums;
using PIF.EBP.Core.CRM;
using System.Configuration;
using PIF.EBP.Application.PortalConfiguration;
using PIF.EBP.Core.Exceptions;

namespace PIF.EBP.Application.Otp.Implementation
{
    public class OtpService : IOtpService
    {
        private readonly ICrmService _crmService;
        private readonly IPortalConfigAppService _portalConfigAppService;
        public OtpService(ICrmService crmService,
            IPortalConfigAppService portalConfigAppService)
        {
            _crmService = crmService;
            _portalConfigAppService = portalConfigAppService;
        }
        public async Task GenerateOtp(OtpType otpOperation, Guid contactId, Guid? invitationId = null)
        {
            var otpLengthRule = _portalConfigAppService
                .RetrievePortalConfiguration(new List<string> { PortalConfigurations.OtpLength })
                .FirstOrDefault();

            int length = 6;

            if(otpLengthRule != null)
            {
                length = Convert.ToInt32(otpLengthRule.Value);
                if (length <= 0)
                {
                    throw new UserFriendlyException("OtpLengthShouldNotBeLessThanOrEqualZero");
                }
            }
            
            var otpCode = OtpHelper.GenerateOTP(length);

            var otpEntity = new Entity();

            otpEntity["pwc_name"] = Enum.GetName(typeof(OtpType), otpOperation);
            otpEntity["pwc_otpcode"] = otpCode.ToString();
            otpEntity["pwc_operationtypetypecode"] = new OptionSetValue((int)otpOperation);

            if (otpOperation == OtpType.Invitation)
            {
                otpEntity["pwc_invitationid"] = new EntityReference(EntityNames.PortalInvitation, invitationId.Value);
            }
            
            otpEntity["pwc_contactid"] = new EntityReference(EntityNames.Contact, contactId);
            

            _crmService.Create(otpEntity, EntityNames.Otp);

            var smsEntity = new Entity();

            var description = "Otp For " + Enum.GetName(typeof(OtpType), otpOperation) + " is " + otpCode;
            smsEntity["subject"] = Enum.GetName(typeof(OtpType), otpOperation);
            smsEntity["description"] = description;
            smsEntity["to"] = new EntityReference(EntityNames.Contact, contactId);
            smsEntity["regardingobjectid"] = new EntityReference(EntityNames.Contact, contactId);
            smsEntity["pwc_contactid"] = new EntityReference(EntityNames.Contact, contactId);

            _crmService.Create(smsEntity, EntityNames.Sms);
        }

        public OtpDto GetOtpEntity(OtpType otpOperation, Guid? contactId, Guid? invitationId = null)
        {
            EntityCollection otpEntities;

            if (otpOperation == OtpType.Invitation)
            {
                otpEntities = _crmService.GetAll(EntityNames.Otp, OtpDto.GetOtpColumns(), invitationId.Value, "pwc_invitationid");
            }
            else
            {
                otpEntities = _crmService.GetAll(EntityNames.Otp, OtpDto.GetOtpColumns(), contactId.Value, "pwc_contactid");
            }

            var otpDto = otpEntities.Entities.Where(x => x.GetAttributeValue<OptionSetValue>("pwc_operationtypetypecode").Value == (int)otpOperation)
                    .Select(x => OtpDto.ConvertToOtpDto(x))
                    .OrderByDescending(x => x.CreatedOn).FirstOrDefault();


            if (Convert.ToBoolean(ConfigurationManager.AppSettings["PassOtp"]))
            {
                var otpLengthRule = _portalConfigAppService
                .RetrievePortalConfiguration(new List<string> { PortalConfigurations.OtpLength })
                .FirstOrDefault();

                var length = Convert.ToInt32(otpLengthRule.Value);
                otpDto.OTPNumber = CreateSameDigitInt(4, length);
            }

            return otpDto;
        }

        private int CreateSameDigitInt(int digit, int n)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < n; i++)
            {
                builder.Append(digit);
            }
            return Convert.ToInt32(builder.ToString());
        }
    }
}
