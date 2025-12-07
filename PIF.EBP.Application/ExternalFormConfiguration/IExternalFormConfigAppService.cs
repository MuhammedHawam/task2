using PIF.EBP.Application.ExternalFormConfiguration.DTOs;
using PIF.EBP.Core.DependencyInjection;
using System;
using System.Collections.Generic;

namespace PIF.EBP.Application.ExternalFormConfiguration
{
    public interface IExternalFormConfigAppService : ITransientDependency
    {
        List<ExternalFormConfigDto> RetrieveExternalFormConfiguration(Guid? Id = null, int? Type = null);
        ExternalFormConfigDto CreateExternalFormConfiguration(ExternalFormConfigDto ExternalFormConfig);
        void UpdateExternalFormConfiguration(ExternalFormConfigDto ExternalFormConfig);
        void DeleteExternalFormConfiguration(string Id);
        List<ExternalFormConfigDto> GetExternalFormConfigurations();
    }
}
