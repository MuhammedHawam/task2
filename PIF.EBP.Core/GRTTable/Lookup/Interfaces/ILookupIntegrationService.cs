using PIF.EBP.Core.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;

namespace PIF.EBP.Core.GRTTable
{
    public interface ILookupIntegrationService : ITransientDependency
    {
        /// <summary>
        /// Get list type definition by external reference code
        /// </summary>
        /// <param name="externalReferenceCode">The external reference code of the list type</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List type definition with entries</returns>
        Task<GRTListTypeDefinitionResponse> GetListTypeDefinitionByExternalReferenceCodeAsync(
            string externalReferenceCode,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get list type entries by list type definition external reference code (paged endpoint).
        /// </summary>
        /// <param name="externalReferenceCode">List type definition external reference code</param>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="scopeGroupId">Optional scope group id (Liferay)</param>
        /// <param name="currentUrl">Optional current URL (Liferay)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task<GRTListTypeEntriesPagedResponse> GetListTypeEntriesByExternalReferenceCodeAsync(
            string externalReferenceCode,
            int page = 1,
            int pageSize = 1000,
            long? scopeGroupId = null,
            string currentUrl = null,
            CancellationToken cancellationToken = default);
    }
}
