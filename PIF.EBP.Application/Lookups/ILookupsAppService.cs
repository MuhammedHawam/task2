using PIF.EBP.Application.Lookups.DTOs;
using PIF.EBP.Core.DependencyInjection;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PIF.EBP.Application.Lookups
{
    public interface ILookupsAppService : ITransientDependency
    {
        Task<List<MasterLookup>> RetrieveLookUpsData(LookupDataRequestDto lookupDataRequestDto);
    }
}
