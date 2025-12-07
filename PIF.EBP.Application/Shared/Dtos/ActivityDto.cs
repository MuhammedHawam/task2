using Microsoft.Xrm.Sdk;
using PIF.EBP.Application.MetaData.DTOs;
using PIF.EBP.Application.Shared.Helpers;
using System;

namespace PIF.EBP.Application.Shared.Dtos
{
    public class SMSDto
    {
        public EntityReferenceDto To { get; set; }
        public string Subject { get; set; }
        public string Describtion { get; set; }
        public EntityReferenceDto Regarding { get; set; }


        public static Entity ConvertToCrmEntity(SMSDto activityDto)
        {
            var activityEntity = new Entity("hexa_sms");
            
            if (activityDto.To != null)
                activityEntity["to"] = new EntityReference(EntityNames.Contact, new Guid(activityDto.To.Id));

            if (activityDto.Regarding != null)
                activityEntity["regardingobjectid"] = new EntityReference(EntityNames.Contact, new Guid(activityDto.Regarding.Id));

            activityEntity["subject"] = activityDto.Subject;
            activityEntity["description"] = activityDto.Describtion;
            

            return activityEntity;
        }

        public static SMSDto ConvertToActivityDto(Entity smsEntity)
        {
            return new SMSDto()
            {
                To = CRMOperations.GetValueByAttributeName<EntityReferenceDto>(smsEntity, "to"),
                Subject = CRMOperations.GetValueByAttributeName<string>(smsEntity, "subject"),
                Describtion = CRMOperations.GetValueByAttributeName<string>(smsEntity, "description"),
                Regarding = CRMOperations.GetValueByAttributeName<EntityReferenceDto>(smsEntity, "regardingobjectid")
            };
        }

        public static string[] GetActivityColumns()
        {
            string[] activityColumns = new string[] { "massege", "otptype", "issent"};
            return activityColumns;
        }
    }
}
