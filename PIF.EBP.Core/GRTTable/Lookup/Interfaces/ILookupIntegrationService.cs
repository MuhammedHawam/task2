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
    }
}
