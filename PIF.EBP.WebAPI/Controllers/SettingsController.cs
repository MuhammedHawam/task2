using PIF.EBP.Application.Contacts;
using PIF.EBP.Application.Contacts.Dtos;
using PIF.EBP.Application.Lookups;
using PIF.EBP.Application.Lookups.DTOs;
using PIF.EBP.Application.Settings;
using PIF.EBP.Application.Settings.DTOs;
using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.Core.Session;
using PIF.EBP.Core.Utilities;
using PIF.EBP.WebAPI.Middleware.ActionFilter;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace PIF.EBP.WebAPI.Controllers
{
    [ApiResponseWrapper]
    [RoutePrefix("Settings")]
    public class SettingsController : BaseController
    {
        private readonly IUserProfileAppService _userProfileAppService;
        private readonly IContactAppService _contactAppService;
        private readonly ISessionService _sessionService;
        private readonly ILookupsAppService _lookupsAppService;

        public SettingsController()
        {
            _userProfileAppService = WindsorContainerProvider.Container.Resolve<IUserProfileAppService>();
            _contactAppService = WindsorContainerProvider.Container.Resolve<IContactAppService>();
            _sessionService = WindsorContainerProvider.Container.Resolve<ISessionService>();
            _lookupsAppService = WindsorContainerProvider.Container.Resolve<ILookupsAppService>();
        }

        [HttpPut]
        [Route("update-user-profile-image")]
        public IHttpActionResult UpdateUserProfileImage(UserProfileImageDto userProfileImageDto)
        {
            _userProfileAppService.UpdateUserProfileImage(userProfileImageDto);

            return Ok(userProfileImageDto);
        }

        [HttpPut]
        [Route("update-user-tutorial")]
        public IHttpActionResult UpdateShowTutorial(UpdateShowTutorialDto updateShowTutorialDto)
        {
            _userProfileAppService.UpdateShowTutorial(updateShowTutorialDto.Value);

            return Ok(updateShowTutorialDto);
        }

        [HttpGet]
        [Route("get-user-profile-data")]
        public IHttpActionResult RetrieveUserProfileData()
        {
            var result = _userProfileAppService.RetrieveUserProfileData();

            return Ok(result);
        }

        [HttpPut]
        [Route("update-user-profile")]
        public async Task<IHttpActionResult> UpdateUserProfile(UserPorfileDto userPorfileDto)
        {
            var result = await _userProfileAppService.UpdateUserProfile(userPorfileDto);

            return Ok(result);
        }

        [HttpGet]
        [Route("get-user-preferences")]
        public IHttpActionResult GetUserPreferences()
        {
            var contactId = _sessionService.GetContactId();
            var result = _contactAppService.GetUserPreferences(contactId);

            return Ok(result);
        }
        [HttpPut]
        [Route("update-preferences")]
        public IHttpActionResult UpdatePreferences(UpdateUserPreferenceDto oUpdateUserPreferenceDto)
        {
            var contactId = _sessionService.GetContactId();
            Guard.AssertArgumentNotNull(oUpdateUserPreferenceDto);
            Guard.AssertArgumentNotNull(contactId);
            _contactAppService.UpdateUserPreferences(contactId, oUpdateUserPreferenceDto);

            return Ok();
        }
        [HttpPost]
        [Route("master-data")]
        public async Task<IHttpActionResult> RetrieveLookUpsData(LookupDataRequestDto lookupDataRequestDto)
        {
            var result = await _lookupsAppService.RetrieveLookUpsData(lookupDataRequestDto);

            return Ok(result);
        }
        [HttpPost]
        [Route("add-qualification")]
        public async Task<IHttpActionResult> AddUserProfileQualification(List<UserProfileEducationDto> userProfileEducationDto)
        {
            var result = await _userProfileAppService.AddUserProfileQualification(userProfileEducationDto);

            return Ok(result);
        }

    }

    public class UpdateShowTutorialDto
    {
        public bool Value { get; set; }
    }
}