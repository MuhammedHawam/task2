using PIF.EBP.Application.EntitiesCache.DTOs;
using PIF.EBP.Application.PortalConfiguration.DTOs;
using PIF.EBP.Core.DependencyInjection;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PIF.EBP.Application.PortalConfiguration
{
    public interface IPortalConfigAppService : ITransientDependency
    {
        Task<CreateOrUpdatePortalConfigDto> CreateOrUpdatePortalConfiguration(CreateOrUpdatePortalConfigDto createOrUpdatePortalConfigDto);
        List<PortalConfigDto> RetrievePortalConfiguration(List<string> keys);
        PortalConfigDto RetrievePortalConfigurationByValue(string value);
    }
}
