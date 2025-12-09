using System.Threading;
using System.Threading.Tasks;
using PIF.EBP.Core.DependencyInjection;

namespace PIF.EBP.Core.GRT
{
    /// <summary>
    /// Integration service for GRT (Government Reference Tables) APIs
    /// </summary>
    public interface IGRTIntegrationService : ITransientDependency
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
        /// Create a new project overview in GRT
        /// </summary>
        /// <param name="request">Project overview data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created project overview response</returns>
        Task<GRTProjectOverviewResponse> CreateProjectOverviewAsync(
            GRTProjectOverviewRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get project overview by ID
        /// </summary>
        /// <param name="id">The ID of the project overview</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Project overview data</returns>
        Task<GRTProjectOverviewResponse> GetProjectOverviewByIdAsync(
            long id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Update project overview by ID
        /// </summary>
        /// <param name="id">The ID of the project overview</param>
        /// <param name="request">Project overview data to update</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated project overview response</returns>
        Task<GRTProjectOverviewResponse> UpdateProjectOverviewAsync(
            long id,
            GRTProjectOverviewRequest request,
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
        Task<GRTUiCyclesPagedResponse> GetCyclesPagedAsync(
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
        Task<GRTDeliveryPlansPagedResponse> GetDeliveryPlansPagedAsync(
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
        /// <returns>Delivery plan data</returns>
        Task<GRTDeliveryPlan> GetDeliveryPlanByIdAsync(
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
        /// Create a new delivery plan in GRT
        /// </summary>
        /// <param name="request">Delivery plan data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created delivery plan response</returns>
        Task<GRTDeliveryPlanResponse> CreateDeliveryPlanAsync(
            GRTDeliveryPlanRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Update delivery plan by ID
        /// </summary>
        /// <param name="id">The ID of the delivery plan</param>
        /// <param name="request">Delivery plan data to update</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated delivery plan response</returns>
        Task<GRTDeliveryPlanResponse> UpdateDeliveryPlanAsync(
            long id,
            GRTDeliveryPlanRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get infrastructure delivery plans by project overview ID
        /// </summary>
        /// <param name="projectOverviewId">The ID of the project overview</param>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated infrastructure delivery plans response</returns>
        Task<GRTInfraDeliveryPlansPagedResponse> GetInfraDeliveryPlansByProjectIdAsync(
            long projectOverviewId,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get infrastructure delivery plan by ID
        /// </summary>
        /// <param name="id">The ID of the infrastructure delivery plan</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Infrastructure delivery plan data</returns>
        Task<GRTInfraDeliveryPlan> GetInfraDeliveryPlanByIdAsync(
            long id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Create a new infrastructure delivery plan in GRT
        /// </summary>
        /// <param name="request">Infrastructure delivery plan data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created infrastructure delivery plan response</returns>
        Task<GRTInfraDeliveryPlanResponse> CreateInfraDeliveryPlanAsync(
            GRTInfraDeliveryPlanRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Update infrastructure delivery plan by ID
        /// </summary>
        /// <param name="id">The ID of the infrastructure delivery plan</param>
        /// <param name="request">Infrastructure delivery plan data to update</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated infrastructure delivery plan response</returns>
        Task<GRTInfraDeliveryPlanResponse> UpdateInfraDeliveryPlanAsync(
            long id,
            GRTInfraDeliveryPlanRequest request,
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
        /// Get infrastructure delivery plan years by delivery plan ID
        /// </summary>
        /// <param name="infraDeliveryPlanId">The ID of the infrastructure delivery plan</param>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated infrastructure delivery plan years response</returns>
        Task<GRTInfraDeliveryPlanYearsPagedResponse> GetInfraDeliveryPlanYearsByPlanIdAsync(
            long infraDeliveryPlanId,
            int page = 1,
            int pageSize = 100,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Create a new infrastructure delivery plan year entry
        /// </summary>
        /// <param name="request">Infrastructure delivery plan year data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created infrastructure delivery plan year response</returns>
        Task<GRTInfraDeliveryPlanYear> CreateInfraDeliveryPlanYearAsync(
            GRTInfraDeliveryPlanYearRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Update infrastructure delivery plan year by ID
        /// </summary>
        /// <param name="id">The ID of the infrastructure delivery plan year</param>
        /// <param name="request">Infrastructure delivery plan year data to update</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated infrastructure delivery plan year response</returns>
        Task<GRTInfraDeliveryPlanYear> UpdateInfraDeliveryPlanYearAsync(
            long id,
            GRTInfraDeliveryPlanYearRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete infrastructure delivery plan year by ID
        /// </summary>
        /// <param name="id">The ID of the infrastructure delivery plan year</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if deleted successfully</returns>
        Task<bool> DeleteInfraDeliveryPlanYearAsync(
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
        Task<GRTLandSalesPagedResponse> GetLandSalesByProjectIdAsync(
            long projectOverviewId,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get land sale by ID
        /// </summary>
        /// <param name="id">The ID of the land sale</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Land sale data</returns>
        Task<GRTLandSale> GetLandSaleByIdAsync(
            long id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Create a new land sale in GRT
        /// </summary>
        /// <param name="request">Land sale data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created land sale response</returns>
        Task<GRTLandSaleResponse> CreateLandSaleAsync(
            GRTLandSaleRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Update land sale by ID
        /// </summary>
        /// <param name="id">The ID of the land sale</param>
        /// <param name="request">Land sale data to update</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated land sale response</returns>
        Task<GRTLandSaleResponse> UpdateLandSaleAsync(
            long id,
            GRTLandSaleRequest request,
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
        Task<GRTCashflowsPagedResponse> GetCashflowsByProjectIdAsync(
            long projectOverviewId,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get cashflow by ID
        /// </summary>
        /// <param name="id">The ID of the cashflow</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Cashflow data</returns>
        Task<GRTCashflow> GetCashflowByIdAsync(
            long id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get GRT budgets paged
        /// </summary>
        /// <param name="poid"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="search"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<GRTBudgetsResponse> GetGRTBudgetsPagedAsync(
            long poid,
            int page = 1,
            int pageSize = 20,
            string search = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get Budget by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<GRTBudgetResponse> GetBudgetByIdAsync(long id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Create a new budget record in GRT.
        /// </summary>
        Task<GRTBudgetResponse> CreateBudgetAsync(
            GRTBudgetRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Update an existing budget record in GRT.
        /// </summary>
        Task<GRTBudgetResponse> UpdateBudgetAsync(
            long id,
            GRTBudgetRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete budget by id.
        /// </summary>
        Task<bool> DeleteBudgetAsync(
            long budgetId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Patch budget data (by external reference code) for large matrix sections.
        /// </summary>
        Task<GRTBudgetResponse> PatchBudgetByExternalReferenceAsync(
            string externalReferenceCode,
            GRTBudgetRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Create a new cashflow in GRT
        /// </summary>
        /// <param name="request">Cashflow data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created cashflow response</returns>
        Task<GRTCashflowResponse> CreateCashflowAsync(
            GRTCashflowRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Update cashflow by ID
        /// </summary>
        /// <param name="id">The ID of the cashflow</param>
        /// <param name="request">Cashflow data to update</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated cashflow response</returns>
        Task<GRTCashflowResponse> UpdateCashflowAsync(
            long id,
            GRTCashflowRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get LOI & HMA (Approved Business Plan) by project overview ID
        /// </summary>
        /// <param name="projectOverviewId">The ID of the project overview</param>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated LOI & HMA response</returns>
        Task<GRTLOIHMAsPagedResponse> GetLOIHMAsByProjectIdAsync(
            long projectOverviewId,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get LOI & HMA by ID
        /// </summary>
        /// <param name="id">The ID of the LOI & HMA</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>LOI & HMA data</returns>
        Task<GRTLOIHMA> GetLOIHMAByIdAsync(
            long id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Create a new LOI & HMA in GRT
        /// </summary>
        /// <param name="request">LOI & HMA data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created LOI & HMA response</returns>
        Task<GRTLOIHMAResponse> CreateLOIHMAAsync(
            GRTLOIHMARequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Update LOI & HMA by project overview ID and LOI & HMA ID
        /// </summary>
        /// <param name="projectOverviewId">The ID of the project overview</param>
        /// <param name="id">The ID of the LOI & HMA</param>
        /// <param name="request">LOI & HMA data to update</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated LOI & HMA response</returns>
        Task<GRTLOIHMAResponse> UpdateLOIHMAAsync(
            long projectOverviewId,
            long id,
            GRTLOIHMARequest request,
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
        Task<GRTMultipleSandUsPagedResponse> GetMultipleSandUsByProjectIdAsync(
            long projectOverviewId,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get Multiple S&U by ID
        /// </summary>
        /// <param name="id">The ID of the Multiple S&U</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Multiple S&U data</returns>
        Task<GRTMultipleSandU> GetMultipleSandUByIdAsync(
            long id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Create a new Multiple S&U in GRT
        /// </summary>
        /// <param name="request">Multiple S&U data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created Multiple S&U response</returns>
        Task<GRTMultipleSandUResponse> CreateMultipleSandUAsync(
            GRTMultipleSandURequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Update Multiple S&U by project overview ID and Multiple S&U ID
        /// </summary>
        /// <param name="projectOverviewId">The ID of the project overview</param>
        /// <param name="id">The ID of the Multiple S&U</param>
        /// <param name="request">Multiple S&U data to update</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated Multiple S&U response</returns>
        Task<GRTMultipleSandUResponse> UpdateMultipleSandUAsync(
            long projectOverviewId,
            long id,
            GRTMultipleSandURequest request,
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


        Task<GRTProjectImpact> GetProjectImpactByIdAsync(
              long id,
              CancellationToken cancellationToken = default);

        Task<GRTProjectImpactResponse> UpdateProjectImpactAsync(
            long id,
            GRTProjectImpactRequest request,
            CancellationToken cancellationToken = default);

        Task<GRTProjectImpactResponse> CreateProjectImpactAsync(
                         GRTProjectImpactRequest request,
                         CancellationToken cancellationToken = default);
    }
}
