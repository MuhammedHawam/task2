using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.Core.GRTTable;
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

        /// <summary>
        /// Get list type entries (paged endpoint) by list type definition external reference code.
        /// </summary>
        Task<GRTListTypeEntriesPagedResponse> GetListTypeEntriesByExternalReferenceCodeAsync(
            string externalReferenceCode,
            int page = 1,
            int pageSize = 1000,
            long? scopeGroupId = null,
            string currentUrl = null,
            CancellationToken cancellationToken = default);
    }
}
