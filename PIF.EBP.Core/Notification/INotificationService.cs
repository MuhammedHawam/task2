using PIF.EBP.Core.DependencyInjection;

namespace PIF.EBP.Core.Notification
{
    public interface INotificationService : ITransientDependency
    {
        void SendSms(string toPhoneNumber, string message);
    }
}
