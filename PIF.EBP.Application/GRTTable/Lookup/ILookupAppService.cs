using PIF.EBP.Core.DependencyInjection;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PIF.EBP.Application.GRTTable
{
    public interface ILookupAppService : ITransientDependency
    {
        /// <summary>
        /// Get lookup entries by external reference code
        /// </summary>
        /// <param name="externalReferenceCode">The external reference code of the lookup</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of lookup entries</returns>
        Task<List<GRTLookupEntryDto>> GetLookupByExternalReferenceCodeAsync(
            string externalReferenceCode,
            CancellationToken cancellationToken = default);
    }
}
