using PIF.EBP.Application.GRT;
using PIF.EBP.Core.GRT;
using PIF.EBP.Core.GRTTable;
using PIF.EBP.Core.GRTTable.ProjectOverview;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PIF.EBP.Application.GRTTable.ProjectOverview.Implementation
{
    /// <summary>
    /// Application service for GRT Project Overview operations
    /// </summary>
    public class ProjectOverviewAppService : IProjectOverviewAppService
    {
        private readonly IProjectOverviewIntegrationService _integrationService;

        public ProjectOverviewAppService(IProjectOverviewIntegrationService integrationService)
        {
            _integrationService = integrationService ?? throw new ArgumentNullException(nameof(integrationService));
        }

        public async Task<ProjectOverviewResponseDto> CreateProjectOverviewAsync(ProjectOverviewDto projectOverview, CancellationToken cancellationToken = default)

        {
            if (projectOverview == null)
            {
                throw new ArgumentNullException(nameof(projectOverview), "Project overview cannot be null");
            }

            try
            {
                // Map application DTO to integration request DTO
                var request = new GRTProjectOverviewRequest
                {
                    ProjectCompanyFullName = projectOverview.ProjectCompanyFullName,
                    LocationCity = projectOverview.LocationCity,
                    ConceptDescription = projectOverview.ConceptDescription,
                    LandSize = projectOverview.LandSize,
                    LandTake = projectOverview.LandTake,
                    DevelopableLand = projectOverview.DevelopableLand,
                    LandValueUsedInIRRCalculation = projectOverview.LandValueUsedInIRRCalculation,
                    TotalFundingRequiredAllSources = projectOverview.TotalFundingRequiredAllSources,
                    Latitude = projectOverview.Latitude,
                    Longitude = projectOverview.Longitude,

                    // Map list entry references - send int directly
                    LastYearOfFundingRequired = projectOverview.LastYearOfFundingRequiredId,
                    DataFilledBasedOnAnApprovedBPByCompanyBoD = !string.IsNullOrEmpty(projectOverview.DataFilledBasedOnAnApprovedBPByCompanyBoD)
                        ? new GRTKeyValue { Key = projectOverview.DataFilledBasedOnAnApprovedBPByCompanyBoD }
                        : null,
                    DataFilledBasedOnAnApprovedBPByPIF = !string.IsNullOrEmpty(projectOverview.DataFilledBasedOnAnApprovedBPByPIF)
                        ? new GRTKeyValue { Key = projectOverview.DataFilledBasedOnAnApprovedBPByPIF }
                        : null,

                    // Management
                    CEO = projectOverview.CEO,
                    CEOIsActing = !string.IsNullOrEmpty(projectOverview.CEOIsActing)
                        ? new GRTKeyValue { Key = projectOverview.CEOIsActing }
                        : null,
                    CFO = projectOverview.CFO,
                    CFOIsActing = !string.IsNullOrEmpty(projectOverview.CFOIsActing)
                        ? new GRTKeyValue { Key = projectOverview.CFOIsActing }
                        : null,
                    CDO = projectOverview.CDO,
                    CDOIsActing = !string.IsNullOrEmpty(projectOverview.CDOIsActing)
                        ? new GRTKeyValue { Key = projectOverview.CDOIsActing }
                        : null,
                    COO = projectOverview.COO,
                    COOIsActing = !string.IsNullOrEmpty(projectOverview.COOIsActing)
                        ? new GRTKeyValue { Key = projectOverview.COOIsActing }
                        : null,
                    CSO = projectOverview.CSO,
                    CSOIsActing = !string.IsNullOrEmpty(projectOverview.CSOIsActing)
                        ? new GRTKeyValue { Key = projectOverview.CSOIsActing }
                        : null,

                    // Project Key Stages
                    CompanyEstablishmentPlanned = !string.IsNullOrEmpty(projectOverview.CompanyEstablishmentPlanned)
                        ? new GRTKeyValue { Key = projectOverview.CompanyEstablishmentPlanned }
                        : null,
                    CompanyEstablishmentActual = !string.IsNullOrEmpty(projectOverview.CompanyEstablishmentActual)
                        ? new GRTKeyValue { Key = projectOverview.CompanyEstablishmentActual }
                        : null,
                    CompanyIncorporationCRPlanned = !string.IsNullOrEmpty(projectOverview.CompanyIncorporationCRPlanned)
                        ? new GRTKeyValue { Key = projectOverview.CompanyIncorporationCRPlanned }
                        : null,
                    CompanyIncorporationCRActual = !string.IsNullOrEmpty(projectOverview.CompanyIncorporationCRActual)
                        ? new GRTKeyValue { Key = projectOverview.CompanyIncorporationCRActual }
                        : null,
                    FirstDesignContractsAwardPlanned = !string.IsNullOrEmpty(projectOverview.FirstDesignContractsAwardPlanned)
                        ? new GRTKeyValue { Key = projectOverview.FirstDesignContractsAwardPlanned }
                        : null,
                    FirstDesignContractsAwardActual = !string.IsNullOrEmpty(projectOverview.FirstDesignContractsAwardActual)
                        ? new GRTKeyValue { Key = projectOverview.FirstDesignContractsAwardActual }
                        : null,
                    FirstInfrastructureAwardPlanned = !string.IsNullOrEmpty(projectOverview.FirstInfrastructureAwardPlanned)
                        ? new GRTKeyValue { Key = projectOverview.FirstInfrastructureAwardPlanned }
                        : null,
                    FirstInfrastructureAwardActual = !string.IsNullOrEmpty(projectOverview.FirstInfrastructureAwardActual)
                        ? new GRTKeyValue { Key = projectOverview.FirstInfrastructureAwardActual }
                        : null,
                    FirstInfrastructureStartDatePlanned = !string.IsNullOrEmpty(projectOverview.FirstInfrastructureStartDatePlanned)
                        ? new GRTKeyValue { Key = projectOverview.FirstInfrastructureStartDatePlanned }
                        : null,
                    FirstInfrastructureStartDateActual = !string.IsNullOrEmpty(projectOverview.FirstInfrastructureStartDateActual)
                        ? new GRTKeyValue { Key = projectOverview.FirstInfrastructureStartDateActual }
                        : null,
                    FirstVerticalConstructionAwardPlanned = !string.IsNullOrEmpty(projectOverview.FirstVerticalConstructionAwardPlanned)
                        ? new GRTKeyValue { Key = projectOverview.FirstVerticalConstructionAwardPlanned }
                        : null,
                    FirstVerticalConstructionAwardActual = !string.IsNullOrEmpty(projectOverview.FirstVerticalConstructionAwardActual)
                        ? new GRTKeyValue { Key = projectOverview.FirstVerticalConstructionAwardActual }
                        : null,
                    FirstVerticalConstructionStartDatePlanned = !string.IsNullOrEmpty(projectOverview.FirstVerticalConstructionStartDatePlanned)
                        ? new GRTKeyValue { Key = projectOverview.FirstVerticalConstructionStartDatePlanned }
                        : null,
                    FirstVerticalConstructionStartDateActual = !string.IsNullOrEmpty(projectOverview.FirstVerticalConstructionStartDateActual)
                        ? new GRTKeyValue { Key = projectOverview.FirstVerticalConstructionStartDateActual }
                        : null,
                    LastInfrastructureCompleteDatePlanned = !string.IsNullOrEmpty(projectOverview.LastInfrastructureCompleteDatePlanned)
                        ? new GRTKeyValue { Key = projectOverview.LastInfrastructureCompleteDatePlanned }
                        : null,
                    LastInfrastructureCompleteDateActual = !string.IsNullOrEmpty(projectOverview.LastInfrastructureCompleteDateActual)
                        ? new GRTKeyValue { Key = projectOverview.LastInfrastructureCompleteDateActual }
                        : null,
                    LastVerticalConstructionCompletePlanned = !string.IsNullOrEmpty(projectOverview.LastVerticalConstructionCompletePlanned)
                        ? new GRTKeyValue { Key = projectOverview.LastVerticalConstructionCompletePlanned }
                        : null,
                    LastVerticalConstructionCompleteActual = !string.IsNullOrEmpty(projectOverview.LastVerticalConstructionCompleteActual)
                        ? new GRTKeyValue { Key = projectOverview.LastVerticalConstructionCompleteActual }
                        : null,
                    OperationsStartDateFirstGuestPlanned = !string.IsNullOrEmpty(projectOverview.OperationsStartDateFirstGuestPlanned)
                        ? new GRTKeyValue { Key = projectOverview.OperationsStartDateFirstGuestPlanned }
                        : null,
                    OperationsStartDateFirstGuestActual = !string.IsNullOrEmpty(projectOverview.OperationsStartDateFirstGuestActual)
                        ? new GRTKeyValue { Key = projectOverview.OperationsStartDateFirstGuestActual }
                        : null,

                    // Key Financials
                    CapRate = projectOverview.CapRate,
                    TerminalValueGrowthRate = projectOverview.TerminalValueGrowthRate,
                    Inflation = projectOverview.Inflation,
                    CostOfEquity = projectOverview.CostOfEquity,
                    WACC = projectOverview.WACC,
                    CostOfDebt = projectOverview.CostOfDebt,
                    DebtToEquityRatio = projectOverview.DebtToEquityRatio,
                    StableReturnOnInvestedCapitalROIC = projectOverview.StableReturnOnInvestedCapitalROIC,
                    TargetDebtServiceCoverageRatioDSCR = projectOverview.TargetDebtServiceCoverageRatioDSCR,

                    // Reference Documents
                    ReferenceDocumentName1 = projectOverview.ReferenceDocumentName1,
                    ReferenceDocumentName2 = projectOverview.ReferenceDocumentName2,
                    ReferenceDocumentName3 = projectOverview.ReferenceDocumentName3,
                    ReferenceDocumentName4 = projectOverview.ReferenceDocumentName4,
                    ReferenceDocumentName5 = projectOverview.ReferenceDocumentName5,
                    ReferenceDocumentName6 = projectOverview.ReferenceDocumentName6,
                    ReferenceDocumentName7 = projectOverview.ReferenceDocumentName7,
                    ReferenceDocumentName8 = projectOverview.ReferenceDocumentName8,
                    ReferenceDocumentName9 = projectOverview.ReferenceDocumentName9,
                    ReferenceDocumentName10 = projectOverview.ReferenceDocumentName10,

                    // Relationships
                    GRTCycleCompanyMapRelationshipId = projectOverview.GRTCycleCompanyMapRelationshipId,
                    GRTCycleCompanyMapRelationshipERC = projectOverview.GRTCycleCompanyMapRelationshipERC
                };

                var response = await _integrationService.CreateProjectOverviewAsync(request, cancellationToken);

                return new ProjectOverviewResponseDto
                {
                    Id = response.Id,
                    ExternalReferenceCode = response.ExternalReferenceCode,
                    DateCreated = DateTime.TryParse(response.DateCreated, out var dateCreated) ? dateCreated : DateTime.Now,
                    DateModified = DateTime.TryParse(response.DateModified, out var dateModified) ? dateModified : DateTime.Now,
                    Success = true,
                    Message = "Project overview created successfully"
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error in GRTAppService.CreateProjectOverviewAsync: {ex.Message}");
                return new ProjectOverviewResponseDto
                {
                    Success = false,
                    Message = $"Error creating project overview: {ex.Message}"
                };
            }
        }

        public async Task<ProjectOverviewDto> GetProjectOverviewAsync(long id, CancellationToken cancellationToken = default)

        {
            if (id <= 0)
            {
                throw new ArgumentException("Project overview ID must be greater than zero", nameof(id));
            }

            try
            {
                var response = await _integrationService.GetProjectOverviewByIdAsync(
                    id,
                    cancellationToken);

                if (response == null)
                {
                    return null;
                }

                // Map integration response DTO to application DTO
                var result = new ProjectOverviewDto
                {
                    ProjectCompanyFullName = response.ProjectCompanyFullName,
                    LocationCity = response.LocationCity,
                    ConceptDescription = response.ConceptDescription,
                    LandSize = response.LandSize,
                    LandTake = response.LandTake,
                    DevelopableLand = response.DevelopableLand,
                    LandValueUsedInIRRCalculation = response.LandValueUsedInIRRCalculation,
                    TotalFundingRequiredAllSources = response.TotalFundingRequiredAllSources,
                    Latitude = response.Latitude,
                    Longitude = response.Longitude,

                    // Map list entry references - extract key from GRTKeyValue and parse as int
                    LastYearOfFundingRequiredId = response.LastYearOfFundingRequired != null &&
                                                  int.TryParse(response.LastYearOfFundingRequired.Key, out var yearId)
                                                  ? yearId : (int?)null,
                    DataFilledBasedOnAnApprovedBPByCompanyBoD = response.DataFilledBasedOnAnApprovedBPByCompanyBoD?.Key,
                    DataFilledBasedOnAnApprovedBPByPIF = response.DataFilledBasedOnAnApprovedBPByPIF?.Key,

                    // Management
                    CEO = response.CEO,
                    CEOIsActing = response.CEOIsActing?.Key,
                    CFO = response.CFO,
                    CFOIsActing = response.CFOIsActing?.Key,
                    CDO = response.CDO,
                    CDOIsActing = response.CDOIsActing?.Key,
                    COO = response.COO,
                    COOIsActing = response.COOIsActing?.Key,
                    CSO = response.CSO,
                    CSOIsActing = response.CSOIsActing?.Key,

                    // Project Key Stages
                    CompanyEstablishmentPlanned = response.CompanyEstablishmentPlanned?.Key,
                    CompanyEstablishmentActual = response.CompanyEstablishmentActual?.Key,
                    CompanyIncorporationCRPlanned = response.CompanyIncorporationCRPlanned?.Key,
                    CompanyIncorporationCRActual = response.CompanyIncorporationCRActual?.Key,
                    FirstDesignContractsAwardPlanned = response.FirstDesignContractsAwardPlanned?.Key,
                    FirstDesignContractsAwardActual = response.FirstDesignContractsAwardActual?.Key,
                    FirstInfrastructureAwardPlanned = response.FirstInfrastructureAwardPlanned?.Key,
                    FirstInfrastructureAwardActual = response.FirstInfrastructureAwardActual?.Key,
                    FirstInfrastructureStartDatePlanned = response.FirstInfrastructureStartDatePlanned?.Key,
                    FirstInfrastructureStartDateActual = response.FirstInfrastructureStartDateActual?.Key,
                    FirstVerticalConstructionAwardPlanned = response.FirstVerticalConstructionAwardPlanned?.Key,
                    FirstVerticalConstructionAwardActual = response.FirstVerticalConstructionAwardActual?.Key,
                    FirstVerticalConstructionStartDatePlanned = response.FirstVerticalConstructionStartDatePlanned?.Key,
                    FirstVerticalConstructionStartDateActual = response.FirstVerticalConstructionStartDateActual?.Key,
                    LastInfrastructureCompleteDatePlanned = response.LastInfrastructureCompleteDatePlanned?.Key,
                    LastInfrastructureCompleteDateActual = response.LastInfrastructureCompleteDateActual?.Key,
                    LastVerticalConstructionCompletePlanned = response.LastVerticalConstructionCompletePlanned?.Key,
                    LastVerticalConstructionCompleteActual = response.LastVerticalConstructionCompleteActual?.Key,
                    OperationsStartDateFirstGuestPlanned = response.OperationsStartDateFirstGuestPlanned?.Key,
                    OperationsStartDateFirstGuestActual = response.OperationsStartDateFirstGuestActual?.Key,

                    // Key Financials
                    CapRate = response.CapRate,
                    TerminalValueGrowthRate = response.TerminalValueGrowthRate,
                    Inflation = response.Inflation,
                    CostOfEquity = response.CostOfEquity,
                    WACC = response.WACC,
                    CostOfDebt = response.CostOfDebt,
                    DebtToEquityRatio = response.DebtToEquityRatio,
                    StableReturnOnInvestedCapitalROIC = response.StableReturnOnInvestedCapitalROIC,
                    TargetDebtServiceCoverageRatioDSCR = response.TargetDebtServiceCoverageRatioDSCR,

                    // Reference Documents
                    ReferenceDocumentName1 = response.ReferenceDocumentName1,
                    ReferenceDocumentName2 = response.ReferenceDocumentName2,
                    ReferenceDocumentName3 = response.ReferenceDocumentName3,
                    ReferenceDocumentName4 = response.ReferenceDocumentName4,
                    ReferenceDocumentName5 = response.ReferenceDocumentName5,
                    ReferenceDocumentName6 = response.ReferenceDocumentName6,
                    ReferenceDocumentName7 = response.ReferenceDocumentName7,
                    ReferenceDocumentName8 = response.ReferenceDocumentName8,
                    ReferenceDocumentName9 = response.ReferenceDocumentName9,
                    ReferenceDocumentName10 = response.ReferenceDocumentName10,

                    // Relationships
                    GRTCycleCompanyMapRelationshipId = response.GRTCycleCompanyMapRelationshipId,
                    GRTCycleCompanyMapRelationshipERC = response.GRTCycleCompanyMapRelationshipERC,
                    AuditEvents = response.AuditEvents
                };

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error in GRTAppService.GetProjectOverviewAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<ProjectOverviewResponseDto> UpdateProjectOverviewAsync(long id, ProjectOverviewDto projectOverview, CancellationToken cancellationToken = default)

        {
            if (id <= 0)
            {
                throw new ArgumentException("Project overview ID must be greater than zero", nameof(id));
            }

            if (projectOverview == null)
            {
                throw new ArgumentNullException(nameof(projectOverview), "Project overview cannot be null");
            }

            try
            {
                // Map application DTO to integration request DTO
                var request = new GRTProjectOverviewRequest
                {
                    ProjectCompanyFullName = projectOverview.ProjectCompanyFullName,
                    LocationCity = projectOverview.LocationCity,
                    ConceptDescription = projectOverview.ConceptDescription,
                    LandSize = projectOverview.LandSize,
                    LandTake = projectOverview.LandTake,
                    DevelopableLand = projectOverview.DevelopableLand,
                    LandValueUsedInIRRCalculation = projectOverview.LandValueUsedInIRRCalculation,
                    TotalFundingRequiredAllSources = projectOverview.TotalFundingRequiredAllSources,
                    Latitude = projectOverview.Latitude,
                    Longitude = projectOverview.Longitude,

                    // Map list entry references - send int directly
                    LastYearOfFundingRequired = projectOverview.LastYearOfFundingRequiredId,
                    DataFilledBasedOnAnApprovedBPByCompanyBoD = !string.IsNullOrEmpty(projectOverview.DataFilledBasedOnAnApprovedBPByCompanyBoD)
                        ? new GRTKeyValue { Key = projectOverview.DataFilledBasedOnAnApprovedBPByCompanyBoD }
                        : null,
                    DataFilledBasedOnAnApprovedBPByPIF = !string.IsNullOrEmpty(projectOverview.DataFilledBasedOnAnApprovedBPByPIF)
                        ? new GRTKeyValue { Key = projectOverview.DataFilledBasedOnAnApprovedBPByPIF }
                        : null,

                    // Management
                    CEO = projectOverview.CEO,
                    CEOIsActing = !string.IsNullOrEmpty(projectOverview.CEOIsActing)
                        ? new GRTKeyValue { Key = projectOverview.CEOIsActing }
                        : null,
                    CFO = projectOverview.CFO,
                    CFOIsActing = !string.IsNullOrEmpty(projectOverview.CFOIsActing)
                        ? new GRTKeyValue { Key = projectOverview.CFOIsActing }
                        : null,
                    CDO = projectOverview.CDO,
                    CDOIsActing = !string.IsNullOrEmpty(projectOverview.CDOIsActing)
                        ? new GRTKeyValue { Key = projectOverview.CDOIsActing }
                        : null,
                    COO = projectOverview.COO,
                    COOIsActing = !string.IsNullOrEmpty(projectOverview.COOIsActing)
                        ? new GRTKeyValue { Key = projectOverview.COOIsActing }
                        : null,
                    CSO = projectOverview.CSO,
                    CSOIsActing = !string.IsNullOrEmpty(projectOverview.CSOIsActing)
                        ? new GRTKeyValue { Key = projectOverview.CSOIsActing }
                        : null,

                    // Project Key Stages
                    CompanyEstablishmentPlanned = !string.IsNullOrEmpty(projectOverview.CompanyEstablishmentPlanned)
                        ? new GRTKeyValue { Key = projectOverview.CompanyEstablishmentPlanned }
                        : null,
                    CompanyEstablishmentActual = !string.IsNullOrEmpty(projectOverview.CompanyEstablishmentActual)
                        ? new GRTKeyValue { Key = projectOverview.CompanyEstablishmentActual }
                        : null,
                    CompanyIncorporationCRPlanned = !string.IsNullOrEmpty(projectOverview.CompanyIncorporationCRPlanned)
                        ? new GRTKeyValue { Key = projectOverview.CompanyIncorporationCRPlanned }
                        : null,
                    CompanyIncorporationCRActual = !string.IsNullOrEmpty(projectOverview.CompanyIncorporationCRActual)
                        ? new GRTKeyValue { Key = projectOverview.CompanyIncorporationCRActual }
                        : null,
                    FirstDesignContractsAwardPlanned = !string.IsNullOrEmpty(projectOverview.FirstDesignContractsAwardPlanned)
                        ? new GRTKeyValue { Key = projectOverview.FirstDesignContractsAwardPlanned }
                        : null,
                    FirstDesignContractsAwardActual = !string.IsNullOrEmpty(projectOverview.FirstDesignContractsAwardActual)
                        ? new GRTKeyValue { Key = projectOverview.FirstDesignContractsAwardActual }
                        : null,
                    FirstInfrastructureAwardPlanned = !string.IsNullOrEmpty(projectOverview.FirstInfrastructureAwardPlanned)
                        ? new GRTKeyValue { Key = projectOverview.FirstInfrastructureAwardPlanned }
                        : null,
                    FirstInfrastructureAwardActual = !string.IsNullOrEmpty(projectOverview.FirstInfrastructureAwardActual)
                        ? new GRTKeyValue { Key = projectOverview.FirstInfrastructureAwardActual }
                        : null,
                    FirstInfrastructureStartDatePlanned = !string.IsNullOrEmpty(projectOverview.FirstInfrastructureStartDatePlanned)
                        ? new GRTKeyValue { Key = projectOverview.FirstInfrastructureStartDatePlanned }
                        : null,
                    FirstInfrastructureStartDateActual = !string.IsNullOrEmpty(projectOverview.FirstInfrastructureStartDateActual)
                        ? new GRTKeyValue { Key = projectOverview.FirstInfrastructureStartDateActual }
                        : null,
                    FirstVerticalConstructionAwardPlanned = !string.IsNullOrEmpty(projectOverview.FirstVerticalConstructionAwardPlanned)
                        ? new GRTKeyValue { Key = projectOverview.FirstVerticalConstructionAwardPlanned }
                        : null,
                    FirstVerticalConstructionAwardActual = !string.IsNullOrEmpty(projectOverview.FirstVerticalConstructionAwardActual)
                        ? new GRTKeyValue { Key = projectOverview.FirstVerticalConstructionAwardActual }
                        : null,
                    FirstVerticalConstructionStartDatePlanned = !string.IsNullOrEmpty(projectOverview.FirstVerticalConstructionStartDatePlanned)
                        ? new GRTKeyValue { Key = projectOverview.FirstVerticalConstructionStartDatePlanned }
                        : null,
                    FirstVerticalConstructionStartDateActual = !string.IsNullOrEmpty(projectOverview.FirstVerticalConstructionStartDateActual)
                        ? new GRTKeyValue { Key = projectOverview.FirstVerticalConstructionStartDateActual }
                        : null,
                    LastInfrastructureCompleteDatePlanned = !string.IsNullOrEmpty(projectOverview.LastInfrastructureCompleteDatePlanned)
                        ? new GRTKeyValue { Key = projectOverview.LastInfrastructureCompleteDatePlanned }
                        : null,
                    LastInfrastructureCompleteDateActual = !string.IsNullOrEmpty(projectOverview.LastInfrastructureCompleteDateActual)
                        ? new GRTKeyValue { Key = projectOverview.LastInfrastructureCompleteDateActual }
                        : null,
                    LastVerticalConstructionCompletePlanned = !string.IsNullOrEmpty(projectOverview.LastVerticalConstructionCompletePlanned)
                        ? new GRTKeyValue { Key = projectOverview.LastVerticalConstructionCompletePlanned }
                        : null,
                    LastVerticalConstructionCompleteActual = !string.IsNullOrEmpty(projectOverview.LastVerticalConstructionCompleteActual)
                        ? new GRTKeyValue { Key = projectOverview.LastVerticalConstructionCompleteActual }
                        : null,
                    OperationsStartDateFirstGuestPlanned = !string.IsNullOrEmpty(projectOverview.OperationsStartDateFirstGuestPlanned)
                        ? new GRTKeyValue { Key = projectOverview.OperationsStartDateFirstGuestPlanned }
                        : null,
                    OperationsStartDateFirstGuestActual = !string.IsNullOrEmpty(projectOverview.OperationsStartDateFirstGuestActual)
                        ? new GRTKeyValue { Key = projectOverview.OperationsStartDateFirstGuestActual }
                        : null,

                    // Key Financials
                    CapRate = projectOverview.CapRate,
                    TerminalValueGrowthRate = projectOverview.TerminalValueGrowthRate,
                    Inflation = projectOverview.Inflation,
                    CostOfEquity = projectOverview.CostOfEquity,
                    WACC = projectOverview.WACC,
                    CostOfDebt = projectOverview.CostOfDebt,
                    DebtToEquityRatio = projectOverview.DebtToEquityRatio,
                    StableReturnOnInvestedCapitalROIC = projectOverview.StableReturnOnInvestedCapitalROIC,
                    TargetDebtServiceCoverageRatioDSCR = projectOverview.TargetDebtServiceCoverageRatioDSCR,

                    // Reference Documents
                    ReferenceDocumentName1 = projectOverview.ReferenceDocumentName1,
                    ReferenceDocumentName2 = projectOverview.ReferenceDocumentName2,
                    ReferenceDocumentName3 = projectOverview.ReferenceDocumentName3,
                    ReferenceDocumentName4 = projectOverview.ReferenceDocumentName4,
                    ReferenceDocumentName5 = projectOverview.ReferenceDocumentName5,
                    ReferenceDocumentName6 = projectOverview.ReferenceDocumentName6,
                    ReferenceDocumentName7 = projectOverview.ReferenceDocumentName7,
                    ReferenceDocumentName8 = projectOverview.ReferenceDocumentName8,
                    ReferenceDocumentName9 = projectOverview.ReferenceDocumentName9,
                    ReferenceDocumentName10 = projectOverview.ReferenceDocumentName10,

                    // Relationships
                    GRTCycleCompanyMapRelationshipId = projectOverview.GRTCycleCompanyMapRelationshipId,
                    GRTCycleCompanyMapRelationshipERC = projectOverview.GRTCycleCompanyMapRelationshipERC
                };

                var response = await _integrationService.UpdateProjectOverviewAsync(id, request, cancellationToken);

                return new ProjectOverviewResponseDto
                {
                    Id = response.Id,
                    ExternalReferenceCode = response.ExternalReferenceCode,
                    DateCreated = DateTime.TryParse(response.DateCreated, out var dateCreated) ? dateCreated : DateTime.Now,
                    DateModified = DateTime.TryParse(response.DateModified, out var dateModified) ? dateModified : DateTime.Now,
                    Success = true,
                    Message = "Project overview updated successfully"
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error in GRTAppService.UpdateProjectOverviewAsync: {ex.Message}");
                return new ProjectOverviewResponseDto
                {
                    Success = false,
                    Message = $"Error updating project overview: {ex.Message}"
                };
            }
        }
    }   
}
