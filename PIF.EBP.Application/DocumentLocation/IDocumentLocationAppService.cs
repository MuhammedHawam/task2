using Microsoft.Xrm.Sdk;
using PIF.EBP.Core.DependencyInjection;
using System;

namespace PIF.EBP.Application.DocumentLocation
{
    public interface IDocumentLocationAppService: ITransientDependency
    {
        void CreateSharePointDocumentLocation(string entityLogicalName, EntityReference oEntityReference);
        string GetDocLocationByRegardingId(Guid regardingId);
    }
}
