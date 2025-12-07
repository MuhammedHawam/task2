using PIF.EBP.Core.DependencyInjection;

namespace PIF.EBP.Core.Session
{
    public interface ISessionService : IScopedDependency
    {
        SessionUser GetCurrentUser();
        string GetContactId();
        string GetCompanyId();
        string GetRoleId();
        string GetEmail();
        string GetLanguage();
        string GetToken();
        bool IsResetToken();
    }
}
