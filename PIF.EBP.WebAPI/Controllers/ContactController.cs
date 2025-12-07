using PIF.EBP.Application.AccessManagement;
using PIF.EBP.Application.CIAMCommunication;
using PIF.EBP.Application.CIAMCommunication.DTOs;
using PIF.EBP.Application.Contacts;
using PIF.EBP.Application.Contacts.Dtos;
using PIF.EBP.Core.Authorization.Users;
using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.Core.Exceptions;
using PIF.EBP.WebAPI.Middleware.ActionFilter;
using PIF.EBP.WebAPI.Middleware.Authorize;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;

namespace PIF.EBP.WebAPI.Controllers
{
    [RoutePrefix("Contact")]
    [ApiResponseWrapper]
    public class ContactController : BaseController
    {
        private readonly IContactAppService _contactAppService;
        private readonly ICIAMUserService _ciamUserService;

        public ContactController()
        {
            _ciamUserService = WindsorContainerProvider.Container.Resolve<ICIAMUserService>();

            _contactAppService = WindsorContainerProvider.Container.Resolve<IContactAppService>();
        }

        [HttpPost]
        [Route("create")]
        public IHttpActionResult Create(ContactCreateDto contactDto)
        {
            var result = _contactAppService.CreateContact(contactDto);
            return Ok(result);
        }

        [HttpPost]
        [Route("V2/Create")]
        public async Task<IHttpActionResult> CreateUser(SCIMContactCreateDto request)
        {
            var result = await _ciamUserService.CreateContact(request);
            return Ok(result);
        }


        [HttpPost]
        [Route("contact-list")]
        public async Task<IHttpActionResult> GetContacts(ContactListReq oContactListReq)
        {
            var result = await _contactAppService.RetrieveContactList(oContactListReq);
            return Ok(result);
        }

        [HttpGet]
        [Route("get-by-company/{companyId}")]
        public IHttpActionResult GetContactsByCompanyId(string companyId)
        {
            var contacts = _contactAppService.GetAllByCompanyId(companyId);
            return Ok(contacts);
        }

        [HttpGet]
        [Route("get-by-id/{contactId}")]
        public async Task<IHttpActionResult> GetContactById(string contactId)
        {
            if (string.IsNullOrEmpty(contactId))
                throw new UserFriendlyException("RequiredParameters", HttpStatusCode.BadRequest);

            var contact = await _contactAppService.GetById(contactId);
            return Ok(contact);
        }

        [HttpPut]
        [Route("update")]
        public async Task<IHttpActionResult> UpdateContact(ContactUpdateDto contact)
        {
            var result = await _contactAppService.Update(contact);
            return Ok(result);
        }


        [HttpDelete]
        [Route("delete")]
        public IHttpActionResult DeleteContact(string contactId)
        {
            var result = _contactAppService.Delete(contactId);
            return Ok(result);
        }

        [HttpPatch]
        [Route("V2/update")]
        public async Task<IHttpActionResult> UpdateSCIMContact(ContactUpdateDto contact)
        {
            var resp = await _ciamUserService.UpdateUserAsync(contact.Id, contact);

            if (resp.IsSuccess)
            {
                return Ok(await _contactAppService.Update(contact));
            }
            return BadRequest(resp.ErrorMessage);
        }

        [HttpDelete]
        [Route("V2/delete")]
        public async Task<IHttpActionResult> DeleteSCIMContact(string contactId, string userName)
        {
            var resp = await _ciamUserService.SetAccountDisabledAsync(contactId, userName, true);

            if (resp.IsSuccess)
            {
                return Ok(_contactAppService.Delete(contactId));
            }
            return  BadRequest(resp.ErrorMessage);
        }

        [HttpPost]
        [Route("pin-contact")]
        public async Task<IHttpActionResult> PinContact(Guid pinContactId, bool isPin)
        {
            if (pinContactId == Guid.Empty)
            {
                throw new UserFriendlyException("NullArgument");
            }
            var result = await _contactAppService.PinContact(pinContactId, isPin);
            return Ok(result);
        }

        [HttpGet]
        [Route("get-contact-image-by-id")]
        public IHttpActionResult GetContactImageById(Guid contactId)
        {
            if (contactId == Guid.Empty)
            {
                throw new UserFriendlyException("NullArgument");
            }
            var result = _contactAppService.RetrieveContactImageById(contactId);

            return Ok(result);
        }
        [HttpGet]
        [OverrideActionFilters]
        [APIKEY]
        [Route("change-email-by-contact-id")]
        public async Task<IHttpActionResult> ChangeEmailByContactId(Guid contactId, string newEmail)
        {
            if (contactId == Guid.Empty || string.IsNullOrEmpty(newEmail))
                throw new UserFriendlyException("NullArgument");

            var deletedResult = await _contactAppService.ChangeEmailByContactId(contactId, newEmail);
            return Ok(deletedResult);
        }

        [HttpGet]
        [Route("get-portal-permission")]
        public IHttpActionResult getPortalPermission()
        {
            var contacts = _contactAppService.GetContactPortalPermission();
            return Ok(contacts);
        }
    }
}
