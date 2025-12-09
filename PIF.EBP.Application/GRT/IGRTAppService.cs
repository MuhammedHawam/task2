using PIF.EBP.Application.GRT.DTOs;
using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.Core.GRT;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PIF.EBP.Application.GRT
{
   
    /// <summary>
    /// Application service for GRT lookups
    /// </summary>
    public interface IGRTAppService : ITransientDependency
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
        /// Create a new project overview
        /// </summary>
        /// <param name="projectOverview">Project overview data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created project overview response</returns>
        Task<GRTProjectOverviewResponseDto> CreateProjectOverviewAsync(
            GRTProjectOverviewDto projectOverview,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get project overview by ID
        /// </summary>
        /// <param name="id">The ID of the project overview</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Project overview data</returns>
        Task<GRTProjectOverviewDto> GetProjectOverviewAsync(
            long id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Update project overview by ID
        /// </summary>
        /// <param name="id">The ID of the project overview</param>
        /// <param name="projectOverview">Project overview data to update</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated project overview response</returns>
        Task<GRTProjectOverviewResponseDto> UpdateProjectOverviewAsync(
            long id,
            GRTProjectOverviewDto projectOverview,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get GRT Cycles for a specific company with combined data from cycle company maps
        /// </summary>
        /// <param name="companyId">The company ID to filter cycles</param>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="search">Search query (optional)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated UI-friendly GRT Cycles response</returns>
        Task<GRTUiCyclesPagedDto> GetCyclesPagedAsync(
            long companyId,
            int page = 1,
            int pageSize = 20,
            string search = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get GRT Delivery Plans by project overview ID with pagination and search
        /// </summary>
        /// <param name="projectOverviewId">The ID of the project overview</param>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="search">Search query (optional)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated GRT Delivery Plans response</returns>
        Task<GRTDeliveryPlansPagedDto> GetDeliveryPlansPagedAsync(
            long projectOverviewId,
            int page = 1,
            int pageSize = 20,
            string search = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get delivery plan by ID
        /// </summary>
        /// <param name="id">The ID of the delivery plan</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Delivery plan details</returns>
        Task<GRTDeliveryPlanDetailDto> GetDeliveryPlanByIdAsync(
            long id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete delivery plan by ID
        /// </summary>
        /// <param name="id">The ID of the delivery plan</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if deleted successfully</returns>
        Task<bool> DeleteDeliveryPlanAsync(
            long id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Create a new delivery plan
        /// </summary>
        /// <param name="deliveryPlan">Delivery plan data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created delivery plan response</returns>
        Task<GRTDeliveryPlanResponseDto> CreateDeliveryPlanAsync(
            GRTDeliveryPlanDto deliveryPlan,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Update delivery plan by ID
        /// </summary>
        /// <param name="id">The ID of the delivery plan</param>
        /// <param name="deliveryPlan">Delivery plan data to update</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated delivery plan response</returns>
        Task<GRTDeliveryPlanResponseDto> UpdateDeliveryPlanAsync(
            long id,
            GRTDeliveryPlanDto deliveryPlan,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get infrastructure delivery plans by project overview ID
        /// </summary>
        /// <param name="projectOverviewId">The ID of the project overview</param>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated infrastructure delivery plans response</returns>
        Task<GRTInfraDeliveryPlansPagedDto> GetInfraDeliveryPlansByProjectIdAsync(
            long projectOverviewId,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get infrastructure delivery plan by ID with years
        /// </summary>
        /// <param name="id">The ID of the infrastructure delivery plan</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Infrastructure delivery plan details with years</returns>
        Task<GRTInfraDeliveryPlanDetailDto> GetInfraDeliveryPlanByIdAsync(
            long id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Create a new infrastructure delivery plan
        /// </summary>
        /// <param name="infraDeliveryPlan">Infrastructure delivery plan data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created infrastructure delivery plan response</returns>
        Task<GRTInfraDeliveryPlanResponseDto> CreateInfraDeliveryPlanAsync(
            GRTInfraDeliveryPlanDto infraDeliveryPlan,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Update infrastructure delivery plan by ID
        /// </summary>
        /// <param name="id">The ID of the infrastructure delivery plan</param>
        /// <param name="infraDeliveryPlan">Infrastructure delivery plan data to update</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated infrastructure delivery plan response</returns>
        Task<GRTInfraDeliveryPlanResponseDto> UpdateInfraDeliveryPlanAsync(
            long id,
            GRTInfraDeliveryPlanDto infraDeliveryPlan,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete infrastructure delivery plan by ID
        /// </summary>
        /// <param name="id">The ID of the infrastructure delivery plan</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if deleted successfully</returns>
        Task<bool> DeleteInfraDeliveryPlanAsync(
            long id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get land sales by project overview ID
        /// </summary>
        /// <param name="projectOverviewId">The ID of the project overview</param>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated land sales response</returns>
        Task<GRTLandSalesPagedDto> GetLandSalesByProjectIdAsync(
            long projectOverviewId,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get land sale by ID
        /// </summary>
        /// <param name="id">The ID of the land sale</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Land sale details</returns>
        Task<GRTLandSaleDetailDto> GetLandSaleByIdAsync(
            long id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Create a new land sale
        /// </summary>
        /// <param name="landSale">Land sale data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created land sale response</returns>
        Task<GRTLandSaleResponseDto> CreateLandSaleAsync(
            GRTLandSaleDto landSale,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Update land sale by ID
        /// </summary>
        /// <param name="id">The ID of the land sale</param>
        /// <param name="landSale">Land sale data to update</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated land sale response</returns>
        Task<GRTLandSaleResponseDto> UpdateLandSaleAsync(
            long id,
            GRTLandSaleDto landSale,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete land sale by ID
        /// </summary>
        /// <param name="id">The ID of the land sale</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if deleted successfully</returns>
        Task<bool> DeleteLandSaleAsync(
            long id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get cashflows by project overview ID
        /// </summary>
        /// <param name="projectOverviewId">The ID of the project overview</param>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated cashflows response</returns>
        Task<GRTCashflowsPagedDto> GetCashflowsByProjectIdAsync(
            long projectOverviewId,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get cashflow by ID
        /// </summary>
        /// <param name="id">The ID of the cashflow</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Cashflow details</returns>
        Task<GRTCashflowDto> GetCashflowByIdAsync(
            long id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get GRT Budgets
        /// </summary>
        /// <param name="poid"> project overview id</param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="search"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<GRTBudgetsPagedDto> GetGRTBudgetsPagedAsync(
            long poid,
            int page = 1, 
            int pageSize = 20,
            string search = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get Budget By Id
        /// </summary>
        /// <param name="id">budget id</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<GRTBudgetResponseDto> GetBudgetByIdAsync(long id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Create a new budget entry.
        /// </summary>
        Task<GRTBudgetResponseDto> CreateBudgetAsync(
            GRTBudgetCreateDto budget,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Update an existing budget entry.
        /// </summary>
        Task<GRTBudgetResponseDto> UpdateBudgetAsync(
            long id,
            GRTBudgetCreateDto budget,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete budget by id.
        /// </summary>
        Task<bool> DeleteBudgetAsync(
            long budgetId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Update budget section matrices (forecast/actual/etc) using the external reference code.
        /// </summary>
        Task<GRTBudgetResponseDto> UpdateBudgetSectionsAsync(
            string externalReferenceCode,
            GRTBudgetSectionsDto sections,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Update cashflow by ID
        /// </summary>
        /// <param name="id">The ID of the cashflow</param>
        /// <param name="cashflow">Cashflow data to update</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated cashflow response</returns>
        Task<GRTCashflowResponseDto> UpdateCashflowAsync(
            long id,
            GRTCashflowDto cashflow,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get LOI & HMA (Approved Business Plan) by project overview ID
        /// </summary>
        /// <param name="projectOverviewId">The ID of the project overview</param>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated LOI & HMA response</returns>
        Task<GRTLOIHMAsPagedDto> GetLOIHMAsByProjectIdAsync(
            long projectOverviewId,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get LOI & HMA by ID
        /// </summary>
        /// <param name="id">The ID of the LOI & HMA</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>LOI & HMA details</returns>
        Task<GRTLOIHMADto> GetLOIHMAByIdAsync(
            long id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Create a new LOI & HMA
        /// </summary>
        /// <param name="loihma">LOI & HMA data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created LOI & HMA response</returns>
        Task<GRTLOIHMAResponseDto> CreateLOIHMAAsync(
            GRTLOIHMADto loihma,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Update LOI & HMA by project overview ID and LOI & HMA ID
        /// </summary>
        /// <param name="projectOverviewId">The ID of the project overview</param>
        /// <param name="id">The ID of the LOI & HMA</param>
        /// <param name="loihma">LOI & HMA data to update</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated LOI & HMA response</returns>
        Task<GRTLOIHMAResponseDto> UpdateLOIHMAAsync(
            long projectOverviewId,
            long id,
            GRTLOIHMADto loihma,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete LOI & HMA by ID
        /// </summary>
        /// <param name="id">The ID of the LOI & HMA</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if deleted successfully</returns>
        Task<bool> DeleteLOIHMAAsync(
            long id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get Multiple S&U by project overview ID
        /// </summary>
        /// <param name="projectOverviewId">The ID of the project overview</param>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated Multiple S&U response</returns>
        Task<GRTMultipleSandUsPagedDto> GetMultipleSandUsByProjectIdAsync(
            long projectOverviewId,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get Multiple S&U by ID
        /// </summary>
        /// <param name="id">The ID of the Multiple S&U</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Multiple S&U details</returns>
        Task<GRTMultipleSandUDetailDto> GetMultipleSandUByIdAsync(
            long id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Create a new Multiple S&U
        /// </summary>
        /// <param name="multipleSandU">Multiple S&U data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created Multiple S&U response</returns>
        Task<GRTMultipleSandUResponseDto> CreateMultipleSandUAsync(
            GRTMultipleSandUDto multipleSandU,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Update Multiple S&U by project overview ID and Multiple S&U ID
        /// </summary>
        /// <param name="projectOverviewId">The ID of the project overview</param>
        /// <param name="id">The ID of the Multiple S&U</param>
        /// <param name="multipleSandU">Multiple S&U data to update</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated Multiple S&U response</returns>
        Task<GRTMultipleSandUResponseDto> UpdateMultipleSandUAsync(
            long projectOverviewId,
            long id,
            GRTMultipleSandUDto multipleSandU,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete Multiple S&U by project overview ID and Multiple S&U ID
        /// </summary>
        /// <param name="projectOverviewId">The ID of the project overview</param>
        /// <param name="id">The ID of the Multiple S&U</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if deleted successfully</returns>
        Task<bool> DeleteMultipleSandUAsync(
            long projectOverviewId,
            long id,
            CancellationToken cancellationToken = default);
            


        Task<GRTProjectImpactDto> GetProjectImpactByIdAsync(
                               long id,
                               CancellationToken cancellationToken = default);

        Task<GRTProjectImpactResponseDto> UpdateProjectImpactAsync(
            long id,
            GRTProjectImpactDto projectImpact,
            CancellationToken cancellationToken = default);

        Task<GRTProjectImpactResponseDto> CreateProjectImpactAsync(
                          GRTProjectImpactDto projectImpact,
                          CancellationToken cancellationToken = default);
    }
}
