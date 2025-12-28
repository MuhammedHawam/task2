using PIF.EBP.Core.Community.DTOs;
using PIF.EBP.Core.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIF.EBP.Core.Community
{
    public interface INotificationService : ITransientDependency
    {
        Task SendEmail(EmailNotificationModel model);
    }
}
