using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.Core.PartnersHub.DTOs;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
namespace PIF.EBP.Core.PartnersHub
{
    /// <summary>
    /// Service interface for PartnersHub Innovation Hub APIs
    /// </summary>
    public interface IPartnersHubService : ITransientDependency
    {
        /// <summary>
        /// Get challenges and campaigns by company IDs with pagination
        /// </summary>
        /// <param name="companyIds">List of company GUIDs</param>
        /// <param name="pageNumber">Page number for pagination</param>
        /// <param name="pageSize">Page size for pagination</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Challenges and campaigns response</returns>
        Task<ChallengeWithCampaignResponse> GetChallengesByCompanyIdAsync(
            List<Guid> companyIds,
            int pageNumber = 0,
            int pageSize = 0,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get activity counts (challenges and campaigns) for multiple companies
        /// </summary>
        /// <param name="companyIds">List of company GUIDs</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Dictionary mapping company ID to their activity counts</returns>
        Task<Dictionary<Guid, (int ChallengesCount, int CampaignsCount)>> GetActivityCountsByCompanyIdsAsync(
            List<Guid> companyIds,
            CancellationToken cancellationToken = default);
    }
}
