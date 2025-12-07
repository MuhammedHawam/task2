using Microsoft.Xrm.Sdk;
using PIF.EBP.Application.MetaData.DTOs;
using PIF.EBP.Application.Shared;
using PIF.EBP.Application.Shared.Helpers;
using System;
using System.Collections.Generic;

namespace PIF.EBP.Application.Contacts.Dtos
{
    public class ContactDto
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string MobilePhone { get; set; }
        public string WorkExperience { get; set; }
        public int InvalidOtpAttempts { get; set; }
        public int InvalidLoginAttempts { get; set; }
        public bool ShowTutorial { get; set; }
        public bool InvitedToPortal { get; set; }
        public byte[] Entityimage { get; set; }
        public DateTime LastLoginDate { get; set; }
        public EntityReferenceDto Company { get; set; }
        public EntityReferenceDto Country { get; set; }
        public EntityReferenceDto City { get; set; }
        public EntityReferenceDto Position { get; set; }
        public EntityReferenceDto ContactManager { get; set; }
        public EntityOptionSetDto Nationality { get; set; }
        public EntityOptionSetDto TwoFactorAuth { get; set; }
        public EntityOptionSetDto UserLanguage { get; set; }
        public EntityOptionSetDto PortalUserStatus { get; set; }
        public List<EntityOptionSetDto> NotificationsPreference { get; set; }


        public static Entity ConvertToCrmEntity(ContactDto contactDto)
        {
            var contactEntity = new Entity("contact");

            contactEntity["contactid"] = contactDto.Id;
            contactEntity["firstname"] = contactDto.FirstName;
            contactEntity["lastname"] = contactDto.LastName;
            contactEntity["fullname"] = contactDto.FullName;
            contactEntity["emailaddress1"] = contactDto.Email;
            contactEntity["mobilephone"] = contactDto.MobilePhone;
            contactEntity["pwc_workexperience"] = contactDto.WorkExperience;
            contactEntity["pwc_invalidotpattempts"] = contactDto.InvalidOtpAttempts;
            contactEntity["hexa_invalidloginattempts"] = contactDto.InvalidLoginAttempts;
            contactEntity["pwc_showtutorial"] = contactDto.ShowTutorial;
            contactEntity["entityimage"] = contactDto.Entityimage;
            contactEntity["hexa_invitetoportal"] = contactDto.InvitedToPortal;

            if (contactDto.Company != null)
                contactEntity["ntw_membershipid"] = new EntityReference(EntityNames.Account, new Guid(contactDto.Company.Id));

            if (contactDto.Country != null)
                contactEntity["pwc_countryid"] = new EntityReference(EntityNames.Country, new Guid(contactDto.Country.Id));

            if (contactDto.City != null)
                contactEntity["pwc_city"] = new EntityReference(EntityNames.City, new Guid(contactDto.City.Id));

            if (contactDto.Position != null)
                contactEntity["pwc_position"] = new EntityReference(EntityNames.Position, new Guid(contactDto.Position.Id));

            if (contactDto.ContactManager != null)
                contactEntity["hexa_contactmanagerid"] = new EntityReference(EntityNames.Contact, new Guid(contactDto.ContactManager.Id));

            if (contactDto.Nationality != null)
            {
                if (int.TryParse(contactDto.Nationality.Value, out int optionSetValue))
                {
                    contactEntity["ntw_nationalityset"] = new OptionSetValue(optionSetValue);
                }
                else
                {
                    throw new InvalidCastException("The Value for the value must be a valid integer.");
                }
            }

            if (contactDto.TwoFactorAuth != null)
            {
                if (int.TryParse(contactDto.TwoFactorAuth.Value, out int optionSetValue))
                {
                    contactEntity["pwc_twofactorauthentication"] = new OptionSetValue(optionSetValue);
                }
                else
                {
                    throw new InvalidCastException("The Value for the value must be a valid integer.");
                }
            }
            if(contactDto.PortalUserStatus!=null)
            {
                if (int.TryParse(contactDto.PortalUserStatus.Value, out int optionSetValue))
                {
                    contactEntity["hexa_portaluserstatustypecode"] = new OptionSetValue(optionSetValue);
                }
                else
                {
                    throw new InvalidCastException("The Value for the value must be a valid integer.");
                }
            }   

            return contactEntity;
        }

        public static ContactDto ConvertToContactDto(Entity contactEntity)
        {
            return new ContactDto()
            {
                Id = CRMOperations.GetValueByAttributeName<Guid>(contactEntity, "contactid"),
                FirstName = CRMOperations.GetValueByAttributeName<string>(contactEntity, "firstname"),
                LastName = CRMOperations.GetValueByAttributeName<string>(contactEntity, "lastname"),
                FullName = CRMOperations.GetValueByAttributeName<string>(contactEntity, "fullname"),
                Email = CRMOperations.GetValueByAttributeName<string>(contactEntity, "emailaddress1"),
                Entityimage = CRMOperations.GetValueByAttributeName<byte[]>(contactEntity, "entityimage"),
                MobilePhone = CRMOperations.GetValueByAttributeName<string>(contactEntity, "mobilephone"),
                WorkExperience = CRMOperations.GetValueByAttributeName<string>(contactEntity, "pwc_workexperience"),
                InvalidOtpAttempts = CRMOperations.GetValueByAttributeName<int>(contactEntity, "pwc_invalidotpattempts"),
                InvalidLoginAttempts = CRMOperations.GetValueByAttributeName<int>(contactEntity, "hexa_invalidloginattempts"),
                ShowTutorial = CRMOperations.GetValueByAttributeName<bool>(contactEntity, "pwc_showtutorial"),
                InvitedToPortal = CRMOperations.GetValueByAttributeName<bool>(contactEntity, "hexa_invitetoportal"),
                Company = CRMOperations.GetValueByAttributeName<EntityReferenceDto>(contactEntity, "ntw_membershipid"),
                Country = CRMOperations.GetValueByAttributeName<EntityReferenceDto>(contactEntity, "pwc_countryid"),
                City = CRMOperations.GetValueByAttributeName<EntityReferenceDto>(contactEntity, "pwc_city"),
                Position = CRMOperations.GetValueByAttributeName<EntityReferenceDto>(contactEntity, "pwc_position"),
                ContactManager = CRMOperations.GetValueByAttributeName<EntityReferenceDto>(contactEntity, "hexa_contactmanagerid"),
                Nationality = CRMOperations.GetValueByAttributeName<EntityOptionSetDto>(contactEntity, "ntw_nationalityset"),
                TwoFactorAuth = CRMOperations.GetValueByAttributeName<EntityOptionSetDto>(contactEntity, "pwc_twofactorauthentication"),
                UserLanguage = CRMOperations.GetValueByAttributeName<EntityOptionSetDto>(contactEntity, "pwc_portallanguagetypecode"),
                PortalUserStatus = CRMOperations.GetValueByAttributeName<EntityOptionSetDto>(contactEntity, "hexa_portaluserstatustypecode"),
                NotificationsPreference=CRMOperations.GetValueByAttributeName<List<EntityOptionSetDto>>(contactEntity, "pwc_notificationspreference"),

            };
        }

        public static string[] GetContactColumns()
        {
            string[] contactColumns = new string[] { "contactid", "firstname", "lastname", "fullname",
                "emailaddress1", "entityimage", "mobilephone", "pwc_position", "pwc_workexperience",
                "pwc_invalidotpattempts", "hexa_invalidloginattempts","hexa_portaluserstatustypecode",
                "pwc_showtutorial", "ntw_membershipid", "pwc_countryid", "pwc_city",
                "hexa_contactmanagerid", "hexa_invitetoportal", "pwc_twofactorauthentication",
                "pwc_portallanguagetypecode", "ntw_nationalityset", "ntw_firstnamearabic", "ntw_lastnamearabic"};
            return contactColumns;
        }
    }
}
