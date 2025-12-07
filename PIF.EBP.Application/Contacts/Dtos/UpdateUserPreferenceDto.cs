using System.Collections.Generic;

namespace PIF.EBP.Application.Contacts.Dtos
{
    public class UpdateUserPreferenceDto
    {
        public int UserLanguage { get; set; }
        public List<int> NotificationPreferences { get; set; }
    }
}
