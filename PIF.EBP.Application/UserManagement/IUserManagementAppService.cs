using PIF.EBP.Application.Shared.AppResponse;
using PIF.EBP.Application.UserManagement.DTOs;
using PIF.EBP.Core.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace PIF.EBP.Application.UserManagement
{
    public interface IUserManagementAppService : ITransientDependency
    {
        Task<ListPagingResponse<UserListResponse>> RetrieveUsersList(UserListReq oUserListReq);
        bool DeactivateUser(Guid associationId);
        UserInviteRes ReinviteUser(UserInviteReq InviteReqlist);
        string ResendInviteUser(UserReSendReq oUserReSendReq);
    }
}
