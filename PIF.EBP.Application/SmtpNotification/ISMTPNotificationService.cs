using PIF.EBP.Application.SMTPNotificaation.DTOs;
using PIF.EBP.Core.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace PIF.EBP.Application.SMTPNotificaation
{
    public interface ISMTPNotificationService : ITransientDependency
    {
        Task<Guid> SendCrmEmailAsync(SendEmailDto emailDto);
    }
}
