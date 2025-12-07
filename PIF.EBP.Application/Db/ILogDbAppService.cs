using PIF.EBP.Core.DependencyInjection;
using System;

namespace PIF.EBP.Application.Db
{
    public interface ILogDbAppService : IScopedDependency
    {
        void CreateLog(Exception exception);
    }
}
