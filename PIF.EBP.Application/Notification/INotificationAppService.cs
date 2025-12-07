using Microsoft.Xrm.Sdk;
using PIF.EBP.Application.Notification.Implementation;
using PIF.EBP.Application.Shared.AppRequest;
using PIF.EBP.Core.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PIF.EBP.Application.Notification
{
    public interface INotificationAppService : ITransientDependency
    {
        Task<NotificationResponse> RetrieveNotifications(PagingRequest pagingRequest);
        Task<int> RetrieveUnreadNotifications();
        Task<string> UpdateNotificationReadStatus(Guid Id);
        string AddNotification(List<Entity> entities);
    }
}
