using PIF.EBP.Application.Requests.DTOs;
using PIF.EBP.Core.DependencyInjection;
using System.Threading.Tasks;

namespace PIF.EBP.Application.Requests
{
    public interface IRequestAppService : ITransientDependency
    {
        Task<HexaRequestListDto_RES> RetrieveRequestList(HexaRequestListDto_REQ oHexaRequestListDto_REQ);
    }
}
