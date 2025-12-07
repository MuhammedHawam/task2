using PIF.EBP.Application.Contacts.Dtos;
using PIF.EBP.Application.Shared.AppResponse;
using PIF.EBP.Core.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PIF.EBP.Application.Contacts
{
    public interface IContactAppService : ITransientDependency
    {
        CreateContactResultDto CreateContact(ContactCreateDto contactDto);
        Task<ListPagingResponse<ContactListResponse>> RetrieveContactList(ContactListReq oContactListDto_REQ);
        IEnumerable<ContactDto> GetAllByCompanyId(string companyId);
        Task<GetContactById> GetById(string contactId);
        Task<GetContactById> GetContactById(string contactId);
        Task<string> Update(ContactUpdateDto contact);
        bool Delete(string contactId);
        UserPreferenceDto GetUserPreferences(string contactId);
        void UpdateUserPreferences(string contactId, UpdateUserPreferenceDto oUpdateUserPreferenceDto);
        void UpdatePrimaryEmail(string contactId, string email);
        Task<bool> PinContact(Guid pinContactId, bool isPin);
        ContactImageDto RetrieveContactImageById(Guid contactId);
        Task<bool> ChangeEmailByContactId(Guid contactId, string newEmail);
        PortalPermissionDto GetContactPortalPermission();
    }
}
