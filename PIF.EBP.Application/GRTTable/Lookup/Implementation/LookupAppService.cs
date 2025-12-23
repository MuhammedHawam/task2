using PIF.EBP.Core.GRTTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PIF.EBP.Application.GRTTable
{
    public class LookupAppService: ILookupAppService
    {
        private readonly ILookupIntegrationService _lookupIntegrationService;

        public LookupAppService(ILookupIntegrationService lookupIntegrationService)
        {
            _lookupIntegrationService = lookupIntegrationService;
        }
        public async Task<List<GRTLookupEntryDto>> GetLookupByExternalReferenceCodeAsync(
        string externalReferenceCode,
        CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(externalReferenceCode))
            {
                throw new ArgumentNullException(nameof(externalReferenceCode), "External reference code cannot be null or empty");
            }

            try
            {
                var response = await _lookupIntegrationService.GetListTypeDefinitionByExternalReferenceCodeAsync(
                    externalReferenceCode,
                    cancellationToken);

                if (response == null || response.ListTypeEntries == null)
                {
                    return new List<GRTLookupEntryDto>();
                }

                // Map integration DTOs to application DTOs
                var result = response.ListTypeEntries.Select(entry => new GRTLookupEntryDto
                {
                    ExternalReferenceCode = entry.ExternalReferenceCode,
                    Id = entry.Id,
                    Key = entry.Key,
                    Name = entry.Name,
                    ArSA = entry.Name_i18n?.ArSA ?? string.Empty,
                    EnUS = entry.Name_i18n?.EnUS ?? string.Empty
                }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error in GRTAppService.GetLookupByExternalReferenceCodeAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<GRTListTypeEntriesPagedResponse> GetListTypeEntriesByExternalReferenceCodeAsync(
            string externalReferenceCode,
            int page = 1,
            int pageSize = 1000,
            long? scopeGroupId = null,
            string currentUrl = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(externalReferenceCode))
            {
                throw new ArgumentNullException(nameof(externalReferenceCode), "External reference code cannot be null or empty");
            }

            return await _lookupIntegrationService.GetListTypeEntriesByExternalReferenceCodeAsync(
                externalReferenceCode,
                page,
                pageSize,
                scopeGroupId,
                currentUrl,
                cancellationToken);
        }
    }
}
