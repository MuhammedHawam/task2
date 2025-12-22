using PIF.EBP.Application.ciamcommunication.DTOs;
using PIF.EBP.Application.CIAMCommunication.DTOs;

using PIF.EBP.Application.Contacts.Dtos;
using PIF.EBP.Application.PortalAdministration.DTOs;
using PIF.EBP.Core.CIAMCommunication.DTOs;
using PIF.EBP.Core.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PIF.EBP.Application.CIAMCommunication.Implmentation.CIAMCommunicationService;

namespace PIF.EBP.Application.CIAMCommunication
{
    public interface ICIAMUserService : ITransientDependency
    {
        Task<CreateContactResultDto> CreateContact(SCIMContactCreateDto contactDto);
        Task<CreateInvitedUserResponse> CreateInvitedUserAsync(CreateSCIMUserRequest request);
        Task<ScimOperationResponse> UpdateUserAsync(
          string userId,
          ContactUpdateDto payload);
        Task<ScimOperationResponse> SetAccountLockedAsync(
           string userId,
           string userName,
           bool lockAccount);
        Task<ScimOperationResponse> SetAccountDisabledAsync(
            string userId,
            string userName,
            bool disableAccount);

        Task<ScimUserListDto> ListAllUsersAsync();
        Task<ScimUserDto> GetUserByUserNameAsync(string userName);   
        Task<IList<ScimUserDto>> GetUsersByEmailAsync(string email); 
        Task<ScimOperationResponse> ResendInvitationAsync(string userId);
        Task<List<ContactListResponse>> GetAllContatcs();
        Task<List<CompanyDto>> RetrievecompaniesByContactId(string ContactId);

    }
}
