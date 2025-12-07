using PIF.EBP.Application.UserManagement;
using PIF.EBP.Application.UserManagement.DTOs;
using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.Core.Exceptions;
using PIF.EBP.WebAPI.Middleware.ActionFilter;
using System;
using System.Threading.Tasks;
using System.Web.Http;

namespace PIF.EBP.WebAPI.Controllers
{
    [RoutePrefix("UserManagement")]
    [ApiResponseWrapper]
    public class UserManagementController : BaseController
    {
        private readonly IUserManagementAppService _userManagementAppService;

        public UserManagementController()
        {
            _userManagementAppService = WindsorContainerProvider.Container.Resolve<IUserManagementAppService>();
        }

        [HttpPost]
        [Route("user-list")]
        public async Task<IHttpActionResult> GetUsersList(UserListReq oUserListReq)
        {
            var result = await _userManagementAppService.RetrieveUsersList(oUserListReq);
            return Ok(result);
        }
        [HttpPost]
        [Route("deactivate-user")]
        public IHttpActionResult DeactivateUser(DeactivateUserReq oDeactivateUserReq)
        {
            if (oDeactivateUserReq == null || oDeactivateUserReq.AssociationId == Guid.Empty)
            {
                throw new UserFriendlyException("NullArgument");
            }
            var result = _userManagementAppService.DeactivateUser(oDeactivateUserReq.AssociationId);
            return Ok(result);
        }
        [HttpPost]
        [Route("resend-invite")]
        public IHttpActionResult ResendInvite(UserReSendReq oUserReSendReq)
        {
            if (oUserReSendReq == null)
            {
                throw new UserFriendlyException("NullArgument");
            }
            var result = _userManagementAppService.ResendInviteUser(oUserReSendReq);
            return Ok(result);
        }
        [HttpPost]
        [Route("reinvite-user")]
        public IHttpActionResult InviteUser(UserInviteReq userInviteReqlist)
        {
            if (userInviteReqlist == null)
            {
                throw new UserFriendlyException("NullArgument", System.Net.HttpStatusCode.BadRequest);
            }
            var result = _userManagementAppService.ReinviteUser(userInviteReqlist);
            return Ok(result);
        }
    }
}