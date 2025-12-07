using PIF.EBP.Core.CIAMCommunication.DTOs;
using PIF.EBP.Core.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIF.EBP.Core.CIAMCommunication
{
    public interface ISCIMCommunicationService : ITransientDependency
    {
        Task<ScimOperationResponse> CreateUserAsync(ScimUserRequest request);
        Task<ScimOperationResponse> UpdateUserAsync(string userId, ScimPatchRequest payload);
        Task<ScimOperationResponse> SetAccountLockedAsync(
            string userId,
            string userName,
            bool lockAccount);
        Task<ScimOperationResponse> SetAccountDisabledAsync(
            string userId,
            string userName,
            bool disableAccount);
        Task<ScimListOperationResponse> GetAllUsersAsync();
        Task<ScimListOperationResponse> GetUserByUserNameAsync(string userName);
        Task<ScimListOperationResponse> GetUserByEmailAsync(string email);
    }
}
