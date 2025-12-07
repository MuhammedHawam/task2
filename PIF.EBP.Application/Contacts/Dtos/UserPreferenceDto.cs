using PIF.EBP.Application.MetaData.DTOs;
using System.Collections.Generic;

namespace PIF.EBP.Application.Contacts.Dtos
{
    public class UserPreferenceDto
    {
        public EntityOptionSetDto UserLanguage { get; set; }
        public List<EntityOptionSetDto> NotificationsPreference { get; set; }
    }
}
