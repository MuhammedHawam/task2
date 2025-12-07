using PIF.EBP.Application.Contacts.Dtos;
using PIF.EBP.Application.Shared.Helpers;

namespace PIF.EBP.Application.InvitationFlow.Dtos
{
    public static class ContactDataMasker
    {
        public static MaskedContactResponseDto MaskContact(ContactDto contact)
        {
            var maskedContact = new MaskedContactResponseDto
            {
                FirstName = contact.FirstName,
                LastName = contact.LastName,    
                Email = DataMaskerHelper.MaskEmail(contact.Email),
                MobilePhone = DataMaskerHelper.MaskPhone(contact.MobilePhone)
            };

            return maskedContact;
        }
    }
}
