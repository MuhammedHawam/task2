using PIF.EBP.Application.Dashboards.DTOs;
using PIF.EBP.Application.Settings.DTOs;
using PIF.EBP.Core.DependencyInjection;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PIF.EBP.Application.Settings
{
    public interface IUserProfileAppService : ITransientDependency
    {
        ContactDetails RetrieveUserDetails();
        Task<string> UpdateUserProfile(UserPorfileDto userPorfileDto);
        string UpdateUserProfileImage(UserProfileImageDto userProfileImageDto);
        UserProfileData RetrieveUserProfileData();
        void UpdateShowTutorial(bool value);
        Task<string> AddUserProfileQualification(List<UserProfileEducationDto> userProfileEducationDto);
    }
}
