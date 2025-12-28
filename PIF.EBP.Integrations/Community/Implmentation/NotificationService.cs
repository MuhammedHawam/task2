using PIF.EBP.Core.Community;
using PIF.EBP.Core.Community.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIF.EBP.Integrations.Community.Implmentation
{
    public class NotificationService : NotificationApiClient, INotificationService
    {

        public Task SendEmail(EmailNotificationModel model)=>
            PostAsync<object>("/notifications/email", model);

     
    }
}
