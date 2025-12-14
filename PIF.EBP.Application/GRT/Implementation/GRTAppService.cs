using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PIF.EBP.Application.GRT.DTOs;
using PIF.EBP.Core.GRT;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PIF.EBP.Application.GRT.Implementation
{
    /// <summary>
    /// Implementation of GRT Application Service
    /// </summary>
    public class GRTAppService : IGRTAppService
    {
        private readonly IGRTIntegrationService _grtIntegrationService;

        public GRTAppService(IGRTIntegrationService grtIntegrationService)
        {
            _grtIntegrationService = grtIntegrationService;
        }

        #region Lookup And Cycles And Project Overview
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
                var response = await _grtIntegrationService.GetListTypeDefinitionByExternalReferenceCodeAsync(
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

        public async Task<GRTProjectOverviewResponseDto> CreateProjectOverviewAsync(
            GRTProjectOverviewDto projectOverview,
            CancellationToken cancellationToken = default)
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

                var response = await _grtIntegrationService.CreateProjectOverviewAsync(request, cancellationToken);

                return new GRTProjectOverviewResponseDto
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
                return new GRTProjectOverviewResponseDto
                {
                    Success = false,
                    Message = $"Error creating project overview: {ex.Message}"
                };
            }
        }

        public async Task<GRTProjectOverviewDto> GetProjectOverviewAsync(
            long id,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Project overview ID must be greater than zero", nameof(id));
            }

            try
            {
                var response = await _grtIntegrationService.GetProjectOverviewByIdAsync(
                    id,
                    cancellationToken);

                if (response == null)
                {
                    return null;
                }

                // Map integration response DTO to application DTO
                var result = new GRTProjectOverviewDto
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
                    GRTCycleCompanyMapRelationshipERC = response.GRTCycleCompanyMapRelationshipERC
                };

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error in GRTAppService.GetProjectOverviewAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<GRTProjectOverviewResponseDto> UpdateProjectOverviewAsync(
            long id,
            GRTProjectOverviewDto projectOverview,
            CancellationToken cancellationToken = default)
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

                var response = await _grtIntegrationService.UpdateProjectOverviewAsync(id, request, cancellationToken);

                return new GRTProjectOverviewResponseDto
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
                return new GRTProjectOverviewResponseDto
                {
                    Success = false,
                    Message = $"Error updating project overview: {ex.Message}"
                };
            }
        }

        public async Task<GRTUiCyclesPagedDto> GetCyclesPagedAsync(
            long companyId,
            int page = 1,
            int pageSize = 20,
            string search = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _grtIntegrationService.GetCyclesPagedAsync(
                    companyId,
                    page,
                    pageSize,
                    search,
                    cancellationToken);

                if (response == null || (response.ActiveCycles == null && response.PreviousCycles == null))
                {
                    return new GRTUiCyclesPagedDto
                    {
                        ActiveCycles = new List<GRTUiCycleDto>(),
                        PreviousCycles = new List<GRTUiCycleDto>(),
                        Page = page,
                        PageSize = pageSize,
                        TotalCount = 0,
                        LastPage = 1
                    };
                }

                // Map active cycles
                var activeCycles = response.ActiveCycles?.Select(cycle => new GRTUiCycleDto
                {
                    CycleId = cycle.CycleId,
                    PoId = cycle.PoId,
                    CompanyId = cycle.CompanyId,
                    CycleName = cycle.CycleName,
                    CycleStartDate = DateTime.TryParse(cycle.CycleStartDate, out var startDate) ? startDate : (DateTime?)null,
                    CycleEndDate = DateTime.TryParse(cycle.CycleEndDate, out var endDate) ? endDate : (DateTime?)null,
                    Status = cycle.Status,
                    RawCycleCompanyStatus = cycle.RawCycleCompanyStatus,
                    RawSystemStatus = cycle.RawSystemStatus
                }).ToList() ?? new List<GRTUiCycleDto>();

                // Map previous cycles (paginated)
                var previousCycles = response.PreviousCycles?.Select(cycle => new GRTUiCycleDto
                {
                    CycleId = cycle.CycleId,
                    PoId = cycle.PoId,
                    CompanyId = cycle.CompanyId,
                    CycleName = cycle.CycleName,
                    CycleStartDate = DateTime.TryParse(cycle.CycleStartDate, out var startDate) ? startDate : (DateTime?)null,
                    CycleEndDate = DateTime.TryParse(cycle.CycleEndDate, out var endDate) ? endDate : (DateTime?)null,
                    Status = cycle.Status,
                    RawCycleCompanyStatus = cycle.RawCycleCompanyStatus,
                    RawSystemStatus = cycle.RawSystemStatus
                }).ToList() ?? new List<GRTUiCycleDto>();

                var result = new GRTUiCyclesPagedDto
                {
                    ActiveCycles = activeCycles,
                    PreviousCycles = previousCycles,
                    Page = response.Page,
                    PageSize = response.PageSize,
                    TotalCount = response.TotalCount,
                    LastPage = response.LastPage
                };

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error in GRTAppService.GetCyclesPagedAsync: {ex.Message}");
                throw;
            }
        }
        #endregion

        #region Delivery Plans
        public async Task<GRTDeliveryPlansPagedDto> GetDeliveryPlansPagedAsync(
            long projectOverviewId,
            int page = 1,
            int pageSize = 20,
            string search = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _grtIntegrationService.GetDeliveryPlansPagedAsync(
                    projectOverviewId,
                    page,
                    pageSize,
                    search,
                    cancellationToken);

                if (response == null || response.Items == null)
                {
                    return new GRTDeliveryPlansPagedDto
                    {
                        Items = new List<GRTDeliveryPlanListDto>(),
                        Page = page,
                        PageSize = pageSize,
                        TotalCount = 0,
                        LastPage = 1
                    };
                }

                // Map integration DTOs to simplified list DTOs
                var result = new GRTDeliveryPlansPagedDto
                {
                    Page = response.Page,
                    PageSize = response.PageSize,
                    TotalCount = response.TotalCount,
                    LastPage = response.LastPage,
                    Items = response.Items.Select(plan => new GRTDeliveryPlanListDto
                    {
                        Id = plan.Id,
                        PlanNumber = plan.PlanNumber,
                        ParcelID = plan.ParcelID,
                        AssetID = plan.AssetID,
                        AssetName = plan.AssetName
                    }).ToList()
                };

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error in GRTAppService.GetDeliveryPlansPagedAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<GRTDeliveryPlanDetailDto> GetDeliveryPlanByIdAsync(
            long id,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Delivery plan ID must be greater than zero", nameof(id));
            }

            try
            {
                var response = await _grtIntegrationService.GetDeliveryPlanByIdAsync(
                    id,
                    cancellationToken);

                if (response == null)
                {
                    return null;
                }

                // Map integration response DTO to application detail DTO
                var result = new GRTDeliveryPlanDetailDto
                {
                    Id = response.Id,
                    ExternalReferenceCode = response.ExternalReferenceCode,
                    DateCreated = DateTime.TryParse(response.DateCreated, out var dateCreated) ? dateCreated : (DateTime?)null,
                    DateModified = DateTime.TryParse(response.DateModified, out var dateModified) ? dateModified : (DateTime?)null,
                    
                    // ASSET IDENTIFICATION
                    PlanNumber = response.PlanNumber,
                    ParcelID = response.ParcelID,
                    AssetID = response.AssetID,
                    AssetName = response.AssetName,
                    AssetType = response.AssetType,
                    SubAsset = response.SubAsset,
                    Description = response.Description,
                    
                    // LOCATION DETAILS
                    City = response.City,
                    Region = response.Region?.Name,
                    RegionKey = response.Region?.Key,
                    ProgramAssetPackage = response.ProgramAssetPackage,
                    
                    // DEVELOPMENT PROFILE
                    TypeOfDevelopment = response.TypeOfDevelopment?.Name,
                    TypeOfDevelopmentKey = response.TypeOfDevelopment?.Key,
                    PrimarySecondaryHomesForResidential = response.PrimarySecondaryHomesForResidential?.Name,
                    PrimarySecondaryHomesForResidentialKey = response.PrimarySecondaryHomesForResidential?.Key,
                    BrandedNonBranded = response.BrandedNonBranded?.Name,
                    BrandedNonBrandedKey = response.BrandedNonBranded?.Key,
                    RetailType = response.RetailType?.Name,
                    RetailTypeKey = response.RetailType?.Key,
                    Phase = response.Phase,
                    ConstructionStart = response.ConstructionStart?.Name,
                    ConstructionStartKey = response.ConstructionStart?.Key,
                    AssetStartOperatingDeliveryDate = response.AssetStartOperatingDeliveryDate?.Name,
                    AssetStartOperatingDeliveryDateKey = response.AssetStartOperatingDeliveryDate?.Key,
                    
                    // ASSET SPECIFICATION
                    ParkingBaysLinkedToAsset = response.ParkingBaysLinkedToAsset,
                    NumberOfFloorsForBuildingsOnly = response.NumberOfFloorsForBuildingsOnly,
                    LandTake = response.LandTake,
                    BUA = response.BUA,
                    GFA = response.GFA,
                    GLA = response.GLA,
                    ResidentialHospitalityKeysLaborStaffRooms = response.ResidentialHospitalityKeysLaborStaffRooms,
                    
                    // FINANCIAL INPUTS
                    DevelopmentIsFundedBy = response.DevelopmentIsFundedBy?.Name,
                    DevelopmentIsFundedByKey = response.DevelopmentIsFundedBy?.Key,
                    RevenueDriver = response.RevenueDriver?.Name,
                    RevenueDriverKey = response.RevenueDriver?.Key,
                    SaleForecastYear = response.SaleForecastYear?.Name,
                    SaleForecastYearKey = response.SaleForecastYear?.Key,
                    SaleStrategy = response.SaleStrategy?.Name,
                    SaleStrategyKey = response.SaleStrategy?.Key,
                    AvgSaleRate = response.AvgSaleRate,
                    LeaseRateOrADRForKeys = response.LeaseRateOrADRForKeys,
                    OccupancyInFirstYearOfOperation = response.OccupancyInFirstYearOfOperation,
                    YearOfStabilization = response.YearOfStabilization?.Name,
                    YearOfStabilizationKey = response.YearOfStabilization?.Key,
                    StableLeaseADR = response.StableLeaseADR,
                    StableOccupancy = response.StableOccupancy,
                    
                    // COST BREAKDOWN
                    ValueOfLandAllocatedToAsset = response.ValueOfLandAllocatedToAsset,
                    ValueOfInfrastructureAllocatedToAsset = response.ValueOfInfrastructureAllocatedToAsset,
                    SoftCost = response.SoftCost,
                    Contingencies = response.Contingencies,
                    VerticalConstructionCost = response.VerticalConstructionCost,
                    
                    // RETURN & PERFORMANCE
                    RevenueProceeds = response.RevenueProceeds,
                    UnleveredIRR = response.UnleveredIRR,
                    VerticalConstructionCostPerSqm = response.VerticalConstructionCostPerSqm,
                    TotalDevelopmentCost = response.TotalDevelopmentCost,
                    TotalDevelopmentCostPerSqm = response.TotalDevelopmentCostPerSqm,
                    
                    // ADDITIONAL NOTES
                    Comments = response.Comments,
                    
                    // Relationships
                    ProjectToDeliveryPlanRelationshipProjectOverviewId = response.ProjectToDeliveryPlanRelationshipProjectOverviewId,
                    ProjectToDeliveryPlanRelationshipProjectOverviewERC = response.ProjectToDeliveryPlanRelationshipProjectOverviewERC
                };

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error in GRTAppService.GetDeliveryPlanByIdAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteDeliveryPlanAsync(
            long id,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Delivery plan ID must be greater than zero", nameof(id));
            }

            try
            {
                return await _grtIntegrationService.DeleteDeliveryPlanAsync(id, cancellationToken);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error in GRTAppService.DeleteDeliveryPlanAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<GRTDeliveryPlanResponseDto> CreateDeliveryPlanAsync(
            GRTDeliveryPlanDto deliveryPlan,
            CancellationToken cancellationToken = default)
        {
            if (deliveryPlan == null)
            {
                throw new ArgumentNullException(nameof(deliveryPlan), "Delivery plan cannot be null");
            }

            try
            {
                // Map application DTO to integration request DTO
                var request = new GRTDeliveryPlanRequest
                {
                    // ASSET IDENTIFICATION
                    PlanNumber = deliveryPlan.PlanNumber,
                    ParcelID = deliveryPlan.ParcelID,
                    AssetID = deliveryPlan.AssetID,
                    AssetName = deliveryPlan.AssetName,
                    AssetType = deliveryPlan.AssetType,
                    SubAsset = deliveryPlan.SubAsset,
                    Description = deliveryPlan.Description,

                    // LOCATION DETAILS
                    City = deliveryPlan.City,
                    Region = BuildKeyValue(deliveryPlan.RegionKey, deliveryPlan.RegionKey),
                    ProgramAssetPackage = deliveryPlan.ProgramAssetPackage,

                    // DEVELOPMENT PROFILE
                    TypeOfDevelopment = BuildKeyValue(deliveryPlan.TypeOfDevelopmentKey, deliveryPlan.TypeOfDevelopmentKey),
                    PrimarySecondaryHomesForResidential = BuildKeyValue(deliveryPlan.PrimarySecondaryHomesForResidentialKey, deliveryPlan.PrimarySecondaryHomesForResidentialKey),
                    BrandedNonBranded = BuildKeyValue(deliveryPlan.BrandedNonBrandedKey, deliveryPlan.BrandedNonBrandedKey),
                    RetailType = BuildKeyValue(deliveryPlan.RetailTypeKey, deliveryPlan.RetailTypeKey),
                    Phase = deliveryPlan.Phase,
                    ConstructionStart = BuildKeyValue(deliveryPlan.ConstructionStartKey, deliveryPlan.ConstructionStartKey),
                    AssetStartOperatingDeliveryDate = BuildKeyValue(deliveryPlan.AssetStartOperatingDeliveryDateKey, deliveryPlan.AssetStartOperatingDeliveryDateKey),

                    // ASSET SPECIFICATION
                    ParkingBaysLinkedToAsset = deliveryPlan.ParkingBaysLinkedToAsset,
                    NumberOfFloorsForBuildingsOnly = deliveryPlan.NumberOfFloorsForBuildingsOnly,
                    LandTake = deliveryPlan.LandTake,
                    BUA = deliveryPlan.BUA,
                    GFA = deliveryPlan.GFA,
                    GLA = deliveryPlan.GLA,
                    ResidentialHospitalityKeysLaborStaffRooms = deliveryPlan.ResidentialHospitalityKeysLaborStaffRooms,

                    // FINANCIAL INPUTS
                    DevelopmentIsFundedBy = BuildKeyValue(deliveryPlan.DevelopmentIsFundedByKey, deliveryPlan.DevelopmentIsFundedByKey),
                    RevenueDriver = BuildKeyValue(deliveryPlan.RevenueDriverKey, deliveryPlan.RevenueDriverKey),
                    SaleForecastYear = BuildKeyValue(deliveryPlan.SaleForecastYearKey, deliveryPlan.SaleForecastYearKey),
                    SaleStrategy = BuildKeyValue(deliveryPlan.SaleStrategyKey, deliveryPlan.SaleStrategyKey),
                    AvgSaleRate = deliveryPlan.AvgSaleRate,
                    LeaseRateOrADRForKeys = deliveryPlan.LeaseRateOrADRForKeys,
                    OccupancyInFirstYearOfOperation = deliveryPlan.OccupancyInFirstYearOfOperation,
                    YearOfStabilization = BuildKeyValue(deliveryPlan.YearOfStabilizationKey, deliveryPlan.YearOfStabilizationKey),
                    StableLeaseADR = deliveryPlan.StableLeaseADR,
                    StableOccupancy = deliveryPlan.StableOccupancy,

                    // COST BREAKDOWN
                    ValueOfLandAllocatedToAsset = deliveryPlan.ValueOfLandAllocatedToAsset,
                    ValueOfInfrastructureAllocatedToAsset = deliveryPlan.ValueOfInfrastructureAllocatedToAsset,
                    SoftCost = deliveryPlan.SoftCost,
                    Contingencies = deliveryPlan.Contingencies,
                    VerticalConstructionCost = deliveryPlan.VerticalConstructionCost,

                    // RETURN & PERFORMANCE
                    RevenueProceeds = deliveryPlan.RevenueProceeds,
                    UnleveredIRR = deliveryPlan.UnleveredIRR,
                    VerticalConstructionCostPerSqm = deliveryPlan.VerticalConstructionCostPerSqm,
                    TotalDevelopmentCost = deliveryPlan.TotalDevelopmentCost,
                    TotalDevelopmentCostPerSqm = deliveryPlan.TotalDevelopmentCostPerSqm,

                    // ADDITIONAL NOTES
                    Comments = deliveryPlan.Comments,

                    // Relationships
                    ProjectToDeliveryPlanRelationshipProjectOverviewId = deliveryPlan.ProjectToDeliveryPlanRelationshipProjectOverviewId,
                    ProjectToDeliveryPlanRelationshipProjectOverviewERC = deliveryPlan.ProjectToDeliveryPlanRelationshipProjectOverviewERC
                };

                var response = await _grtIntegrationService.CreateDeliveryPlanAsync(request, cancellationToken);

                return new GRTDeliveryPlanResponseDto
                {
                    Id = response.Id,
                    ExternalReferenceCode = response.ExternalReferenceCode,
                    DateCreated = DateTime.TryParse(response.DateCreated, out var dateCreated) ? dateCreated : DateTime.Now,
                    DateModified = DateTime.TryParse(response.DateModified, out var dateModified) ? dateModified : DateTime.Now,
                    Success = true,
                    Message = "Delivery plan created successfully"
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error in GRTAppService.CreateDeliveryPlanAsync: {ex.Message}");
                return new GRTDeliveryPlanResponseDto
                {
                    Success = false,
                    Message = $"Error creating delivery plan: {ex.Message}"
                };
            }
        }

        public async Task<GRTDeliveryPlanResponseDto> UpdateDeliveryPlanAsync(
            long id,
            GRTDeliveryPlanDto deliveryPlan,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Delivery plan ID must be greater than zero", nameof(id));
            }

            if (deliveryPlan == null)
            {
                throw new ArgumentNullException(nameof(deliveryPlan), "Delivery plan cannot be null");
            }

            try
            {
                // Map application DTO to integration request DTO
                var request = new GRTDeliveryPlanRequest
                {
                    // ASSET IDENTIFICATION
                    PlanNumber = deliveryPlan.PlanNumber,
                    ParcelID = deliveryPlan.ParcelID,
                    AssetID = deliveryPlan.AssetID,
                    AssetName = deliveryPlan.AssetName,
                    AssetType = deliveryPlan.AssetType,
                    SubAsset = deliveryPlan.SubAsset,
                    Description = deliveryPlan.Description,

                    // LOCATION DETAILS
                    City = deliveryPlan.City,
                    Region = BuildKeyValue(deliveryPlan.RegionKey, deliveryPlan.RegionKey),
                    ProgramAssetPackage = deliveryPlan.ProgramAssetPackage,

                    // DEVELOPMENT PROFILE
                    TypeOfDevelopment = BuildKeyValue(deliveryPlan.TypeOfDevelopmentKey, deliveryPlan.TypeOfDevelopmentKey),
                    PrimarySecondaryHomesForResidential = BuildKeyValue(deliveryPlan.PrimarySecondaryHomesForResidentialKey, deliveryPlan.PrimarySecondaryHomesForResidentialKey),
                    BrandedNonBranded = BuildKeyValue(deliveryPlan.BrandedNonBrandedKey, deliveryPlan.BrandedNonBrandedKey),
                    RetailType = BuildKeyValue(deliveryPlan.RetailTypeKey, deliveryPlan.RetailTypeKey),
                    Phase = deliveryPlan.Phase,
                    ConstructionStart = BuildKeyValue(deliveryPlan.ConstructionStartKey, deliveryPlan.ConstructionStartKey),
                    AssetStartOperatingDeliveryDate = BuildKeyValue(deliveryPlan.AssetStartOperatingDeliveryDateKey, deliveryPlan.AssetStartOperatingDeliveryDateKey),

                    // ASSET SPECIFICATION
                    ParkingBaysLinkedToAsset = deliveryPlan.ParkingBaysLinkedToAsset,
                    NumberOfFloorsForBuildingsOnly = deliveryPlan.NumberOfFloorsForBuildingsOnly,
                    LandTake = deliveryPlan.LandTake,
                    BUA = deliveryPlan.BUA,
                    GFA = deliveryPlan.GFA,
                    GLA = deliveryPlan.GLA,
                    ResidentialHospitalityKeysLaborStaffRooms = deliveryPlan.ResidentialHospitalityKeysLaborStaffRooms,

                    // FINANCIAL INPUTS
                    DevelopmentIsFundedBy = BuildKeyValue(deliveryPlan.DevelopmentIsFundedByKey, deliveryPlan.DevelopmentIsFundedByKey),
                    RevenueDriver = BuildKeyValue(deliveryPlan.RevenueDriverKey, deliveryPlan.RevenueDriverKey),
                    SaleForecastYear = BuildKeyValue(deliveryPlan.SaleForecastYearKey, deliveryPlan.SaleForecastYearKey),
                    SaleStrategy = BuildKeyValue(deliveryPlan.SaleStrategyKey, deliveryPlan.SaleStrategyKey),
                    AvgSaleRate = deliveryPlan.AvgSaleRate,
                    LeaseRateOrADRForKeys = deliveryPlan.LeaseRateOrADRForKeys,
                    OccupancyInFirstYearOfOperation = deliveryPlan.OccupancyInFirstYearOfOperation,
                    YearOfStabilization = BuildKeyValue(deliveryPlan.YearOfStabilizationKey, deliveryPlan.YearOfStabilizationKey),
                    StableLeaseADR = deliveryPlan.StableLeaseADR,
                    StableOccupancy = deliveryPlan.StableOccupancy,

                    // COST BREAKDOWN
                    ValueOfLandAllocatedToAsset = deliveryPlan.ValueOfLandAllocatedToAsset,
                    ValueOfInfrastructureAllocatedToAsset = deliveryPlan.ValueOfInfrastructureAllocatedToAsset,
                    SoftCost = deliveryPlan.SoftCost,
                    Contingencies = deliveryPlan.Contingencies,
                    VerticalConstructionCost = deliveryPlan.VerticalConstructionCost,

                    // RETURN & PERFORMANCE
                    RevenueProceeds = deliveryPlan.RevenueProceeds,
                    UnleveredIRR = deliveryPlan.UnleveredIRR,
                    VerticalConstructionCostPerSqm = deliveryPlan.VerticalConstructionCostPerSqm,
                    TotalDevelopmentCost = deliveryPlan.TotalDevelopmentCost,
                    TotalDevelopmentCostPerSqm = deliveryPlan.TotalDevelopmentCostPerSqm,

                    // ADDITIONAL NOTES
                    Comments = deliveryPlan.Comments,

                    // Relationships
                    ProjectToDeliveryPlanRelationshipProjectOverviewId = deliveryPlan.ProjectToDeliveryPlanRelationshipProjectOverviewId,
                    ProjectToDeliveryPlanRelationshipProjectOverviewERC = deliveryPlan.ProjectToDeliveryPlanRelationshipProjectOverviewERC
                };

                var response = await _grtIntegrationService.UpdateDeliveryPlanAsync(id, request, cancellationToken);

                return new GRTDeliveryPlanResponseDto
                {
                    Id = response.Id,
                    ExternalReferenceCode = response.ExternalReferenceCode,
                    DateCreated = DateTime.TryParse(response.DateCreated, out var dateCreated) ? dateCreated : DateTime.Now,
                    DateModified = DateTime.TryParse(response.DateModified, out var dateModified) ? dateModified : DateTime.Now,
                    Success = true,
                    Message = "Delivery plan updated successfully"
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error in GRTAppService.UpdateDeliveryPlanAsync: {ex.Message}");
                return new GRTDeliveryPlanResponseDto
                {
                    Success = false,
                    Message = $"Error updating delivery plan: {ex.Message}"
                };
            }
        }
        #endregion

        #region Infra Delivery Plans
        public async Task<GRTInfraDeliveryPlansPagedDto> GetInfraDeliveryPlansByProjectIdAsync(
            long projectOverviewId,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _grtIntegrationService.GetInfraDeliveryPlansByProjectIdAsync(
                    projectOverviewId,
                    page,
                    pageSize,
                    cancellationToken);

                if (response == null || response.Items == null)
                {
                    return new GRTInfraDeliveryPlansPagedDto
                    {
                        Items = new List<GRTInfraDeliveryPlanListDto>(),
                        Page = page,
                        PageSize = pageSize,
                        TotalCount = 0,
                        LastPage = 1
                    };
                }

                var result = new GRTInfraDeliveryPlansPagedDto
                {
                    Page = response.Page,
                    PageSize = response.PageSize,
                    TotalCount = response.TotalCount,
                    LastPage = response.LastPage,
                    Items = response.Items.Select(plan => new GRTInfraDeliveryPlanListDto
                    {
                        Id = plan.Id,
                        InfrastructureType = plan.InfrastructureType?.Name,
                        InfrastructureTypeKey = plan.InfrastructureType?.Key,
                        InfrastructureSector = plan.InfrastructureSector?.Name,
                        InfrastructureSectorKey = plan.InfrastructureSector?.Key,
                        Total = plan.Total
                    }).ToList()
                };

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error in GRTAppService.GetInfraDeliveryPlansByProjectIdAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<GRTInfraDeliveryPlanDetailDto> GetInfraDeliveryPlanByIdAsync(
            long id,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Infrastructure delivery plan ID must be greater than zero", nameof(id));
            }

            try
            {
                // Get the infrastructure delivery plan
                var infraPlan = await _grtIntegrationService.GetInfraDeliveryPlanByIdAsync(id, cancellationToken);

                if (infraPlan == null)
                {
                    return null;
                }

                // Get the years for this plan
                var yearsResponse = await _grtIntegrationService.GetInfraDeliveryPlanYearsByPlanIdAsync(id, 1, 100, cancellationToken);

                var result = new GRTInfraDeliveryPlanDetailDto
                {
                    Id = infraPlan.Id,
                    ExternalReferenceCode = infraPlan.ExternalReferenceCode,
                    DateCreated = DateTime.TryParse(infraPlan.DateCreated, out var dateCreated) ? dateCreated : (DateTime?)null,
                    DateModified = DateTime.TryParse(infraPlan.DateModified, out var dateModified) ? dateModified : (DateTime?)null,
                    InfrastructureType = infraPlan.InfrastructureType?.Name,
                    InfrastructureTypeKey = infraPlan.InfrastructureType?.Key,
                    InfrastructureSector = infraPlan.InfrastructureSector?.Name,
                    InfrastructureSectorKey = infraPlan.InfrastructureSector?.Key,
                    Total = infraPlan.Total,
                    ProjectToInfraDeliveryPlanRelationshipProjectOverviewId = infraPlan.ProjectToInfraDeliveryPlanRelationshipProjectOverviewId,
                    ProjectToInfraDeliveryPlanRelationshipProjectOverviewERC = infraPlan.ProjectToInfraDeliveryPlanRelationshipProjectOverviewERC,
                    Years = yearsResponse?.Items?.Select(year => new GRTInfraDeliveryPlanYearDto
                    {
                        Id = year.Id,
                        ActualPlanned = year.ActualPlanned?.Name,
                        ActualPlannedKey = year.ActualPlanned?.Key,
                        Year = year.Year?.Name,
                        YearKey = year.Year?.Key,
                        Amount = year.Amount
                    }).ToList() ?? new List<GRTInfraDeliveryPlanYearDto>()
                };

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error in GRTAppService.GetInfraDeliveryPlanByIdAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<GRTInfraDeliveryPlanResponseDto> CreateInfraDeliveryPlanAsync(
            GRTInfraDeliveryPlanDto infraDeliveryPlan,
            CancellationToken cancellationToken = default)
        {
            if (infraDeliveryPlan == null)
            {
                throw new ArgumentNullException(nameof(infraDeliveryPlan), "Infrastructure delivery plan cannot be null");
            }

            try
            {
                // Create the infrastructure delivery plan
                var request = new GRTInfraDeliveryPlanRequest
                {

                    InfrastructureType = BuildKeyValue(infraDeliveryPlan.InfrastructureTypeKey, infraDeliveryPlan.InfrastructureTypeKey),
                    InfrastructureSector = BuildKeyValue(infraDeliveryPlan.InfrastructureSectorKey, infraDeliveryPlan.InfrastructureSectorKey),
                    Total = infraDeliveryPlan.Years?.Where(y => y?.Amount != null).Sum(y => y.Amount.Value),
                    ProjectToInfraDeliveryPlanRelationshipProjectOverviewId = infraDeliveryPlan.ProjectToInfraDeliveryPlanRelationshipProjectOverviewId,
                    ProjectToInfraDeliveryPlanRelationshipProjectOverviewERC = infraDeliveryPlan.ProjectToInfraDeliveryPlanRelationshipProjectOverviewERC
                };

                var response = await _grtIntegrationService.CreateInfraDeliveryPlanAsync(request, cancellationToken);

                // Create year entries if provided
                if (infraDeliveryPlan.Years != null && infraDeliveryPlan.Years.Any())
                {
                    foreach (var year in infraDeliveryPlan.Years)
                    {
                        var yearRequest = new GRTInfraDeliveryPlanYearRequest
                        {
                            ActualPlanned = BuildKeyValue(year.ActualPlannedKey, year.ActualPlannedKey),
                            Year = BuildKeyValue(year.YearKey, year.YearKey),

                            Amount = year.Amount,
                            InfraDeliveryPlanToYearsRelationshipInfraDeliveryPlanId = response.Id,
                            InfraDeliveryPlanToYearsRelationshipInfraDeliveryPlanERC = response.ExternalReferenceCode
                        };

                        await _grtIntegrationService.CreateInfraDeliveryPlanYearAsync(yearRequest, cancellationToken);
                    }
                }

                return new GRTInfraDeliveryPlanResponseDto
                {
                    Id = response.Id,
                    ExternalReferenceCode = response.ExternalReferenceCode,
                    DateCreated = DateTime.TryParse(response.DateCreated, out var dateCreated) ? dateCreated : DateTime.Now,
                    DateModified = DateTime.TryParse(response.DateModified, out var dateModified) ? dateModified : DateTime.Now,
                    Success = true,
                    Message = "Infrastructure delivery plan created successfully"
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error in GRTAppService.CreateInfraDeliveryPlanAsync: {ex.Message}");
                return new GRTInfraDeliveryPlanResponseDto
                {
                    Success = false,
                    Message = $"Error creating infrastructure delivery plan: {ex.Message}"
                };
            }
        }

        public async Task<GRTInfraDeliveryPlanResponseDto> UpdateInfraDeliveryPlanAsync(
            long id,
            GRTInfraDeliveryPlanDto infraDeliveryPlan,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Infrastructure delivery plan ID must be greater than zero", nameof(id));
            }

            if (infraDeliveryPlan == null)
            {
                throw new ArgumentNullException(nameof(infraDeliveryPlan), "Infrastructure delivery plan cannot be null");
            }

            try
            {
                // Fetch current plan to get a stable ERC for year relationship writes.
                var currentPlan = await _grtIntegrationService.GetInfraDeliveryPlanByIdAsync(id, cancellationToken);
                var planErc = currentPlan?.ExternalReferenceCode;
                if (string.IsNullOrWhiteSpace(planErc))
                {
                    throw new Exception("Unable to resolve Infrastructure Delivery Plan external reference code.");
                }

                // Get existing years
                var existingYears = await _grtIntegrationService.GetInfraDeliveryPlanYearsByPlanIdAsync(id, 1, 100, cancellationToken);

                // Update or create year entries
                if (infraDeliveryPlan.Years != null && infraDeliveryPlan.Years.Any())
                {
                    foreach (var year in infraDeliveryPlan.Years)
                    {
                        var yearRequest = new GRTInfraDeliveryPlanYearRequest
                        {
                            ActualPlanned = BuildKeyValue(year.ActualPlannedKey, year.ActualPlannedKey),
                            Year = BuildKeyValue(year.YearKey, year.YearKey),
                            Amount = year.Amount,
                            InfraDeliveryPlanToYearsRelationshipInfraDeliveryPlanId = id,
                            InfraDeliveryPlanToYearsRelationshipInfraDeliveryPlanERC = planErc
                        };

                        if (year.Id.HasValue && year.Id.Value > 0)
                        {
                            // Update existing year
                            await _grtIntegrationService.UpdateInfraDeliveryPlanYearAsync(year.Id.Value, yearRequest, cancellationToken);
                        }
                        else
                        {
                            // Create new year
                            await _grtIntegrationService.CreateInfraDeliveryPlanYearAsync(yearRequest, cancellationToken);
                        }
                    }
                }

                // Delete years that are no longer in the list
                if (existingYears?.Items != null)
                {
                    var yearIdsToKeep = infraDeliveryPlan.Years?.Where(y => y.Id.HasValue).Select(y => y.Id.Value).ToList() ?? new List<long>();
                    var yearsToDelete = existingYears.Items.Where(y => !yearIdsToKeep.Contains(y.Id)).ToList();

                    foreach (var yearToDelete in yearsToDelete)
                    {
                        await _grtIntegrationService.DeleteInfraDeliveryPlanYearAsync(yearToDelete.Id, cancellationToken);
                    }
                }

                // Recalculate and persist total (grid reads this field).
                var request = new GRTInfraDeliveryPlanRequest
                {
                    InfrastructureType = BuildKeyValue(infraDeliveryPlan.InfrastructureTypeKey, infraDeliveryPlan.InfrastructureTypeKey),
                    InfrastructureSector = BuildKeyValue(infraDeliveryPlan.InfrastructureSectorKey, infraDeliveryPlan.InfrastructureSectorKey),
                    Total = infraDeliveryPlan.Years?.Where(y => y?.Amount != null).Sum(y => y.Amount.Value),
                    ProjectToInfraDeliveryPlanRelationshipProjectOverviewId = infraDeliveryPlan.ProjectToInfraDeliveryPlanRelationshipProjectOverviewId,
                    ProjectToInfraDeliveryPlanRelationshipProjectOverviewERC = infraDeliveryPlan.ProjectToInfraDeliveryPlanRelationshipProjectOverviewERC
                };

                var response = await _grtIntegrationService.UpdateInfraDeliveryPlanAsync(id, request, cancellationToken);

                return new GRTInfraDeliveryPlanResponseDto
                {
                    Id = response.Id,
                    ExternalReferenceCode = response.ExternalReferenceCode,
                    DateCreated = DateTime.TryParse(response.DateCreated, out var dateCreated) ? dateCreated : DateTime.Now,
                    DateModified = DateTime.TryParse(response.DateModified, out var dateModified) ? dateModified : DateTime.Now,
                    Success = true,
                    Message = "Infrastructure delivery plan updated successfully"
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error in GRTAppService.UpdateInfraDeliveryPlanAsync: {ex.Message}");
                return new GRTInfraDeliveryPlanResponseDto
                {
                    Success = false,
                    Message = $"Error updating infrastructure delivery plan: {ex.Message}"
                };
            }
        }

        public async Task<bool> DeleteInfraDeliveryPlanAsync(
            long id,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Infrastructure delivery plan ID must be greater than zero", nameof(id));
            }

            try
            {
                // First, delete all associated years
                var years = await _grtIntegrationService.GetInfraDeliveryPlanYearsByPlanIdAsync(id, 1, 100, cancellationToken);
                if (years?.Items != null)
                {
                    foreach (var year in years.Items)
                    {
                        await _grtIntegrationService.DeleteInfraDeliveryPlanYearAsync(year.Id, cancellationToken);
                    }
                }

                // Then delete the plan itself
                return await _grtIntegrationService.DeleteInfraDeliveryPlanAsync(id, cancellationToken);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error in GRTAppService.DeleteInfraDeliveryPlanAsync: {ex.Message}");
                throw;
            }
        }
        #endregion

        #region Land Sales
        public async Task<GRTLandSalesPagedDto> GetLandSalesByProjectIdAsync(
            long projectOverviewId,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _grtIntegrationService.GetLandSalesByProjectIdAsync(
                    projectOverviewId,
                    page,
                    pageSize,
                    cancellationToken);

                if (response == null || response.Items == null)
                {
                    return new GRTLandSalesPagedDto
                    {
                        Items = new List<GRTLandSaleListDto>(),
                        Page = page,
                        PageSize = pageSize,
                        TotalCount = 0,
                        LastPage = 1
                    };
                }

                var result = new GRTLandSalesPagedDto
                {
                    Page = response.Page,
                    PageSize = response.PageSize,
                    TotalCount = response.TotalCount,
                    LastPage = response.LastPage,
                    Items = response.Items.Select(ls => new GRTLandSaleListDto
                    {
                        Id = ls.Id,
                        PlotName = ls.PlotName,
                        LandUse = ls.LandUse?.Name,
                        LandType = ls.LandType?.Name,
                        City = ls.City,
                        Region = ls.Region?.Name
                    }).ToList()
                };

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error in GRTAppService.GetLandSalesByProjectIdAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<GRTLandSaleDetailDto> GetLandSaleByIdAsync(
            long id,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Land sale ID must be greater than zero", nameof(id));
            }

            try
            {
                var response = await _grtIntegrationService.GetLandSaleByIdAsync(
                    id,
                    cancellationToken);

                if (response == null)
                {
                    return null;
                }

                var result = new GRTLandSaleDetailDto
                {
                    Id = response.Id,
                    ExternalReferenceCode = response.ExternalReferenceCode,
                    DateCreated = DateTime.TryParse(response.DateCreated, out var dateCreated) ? dateCreated : (DateTime?)null,
                    DateModified = DateTime.TryParse(response.DateModified, out var dateModified) ? dateModified : (DateTime?)null,
                    PlotName = response.PlotName,
                    LandUse = response.LandUse?.Name,
                    LandUseKey = response.LandUse?.Key,
                    LandType = response.LandType?.Name,
                    LandTypeKey = response.LandType?.Key,
                    City = response.City,
                    Region = response.Region?.Name,
                    RegionKey = response.Region?.Key,
                    RestrictedDevelopmentToSpecificCriteria = response.RestrictedDevelopmentToSpecificCriteria?.Name,
                    RestrictedDevelopmentToSpecificCriteriaKey = response.RestrictedDevelopmentToSpecificCriteria?.Key,
                    SaleLease = response.SaleLease?.Name,
                    SaleLeaseKey = response.SaleLease?.Key,
                    NumberOfPlots = response.NumberOfPlots,
                    TotalLandArea = response.TotalLandArea,
                    AvgSaleLeaseRate = response.AvgSaleLeaseRate,
                    YearOfSaleLeaseStart = response.YearOfSaleLeaseStart?.Name,
                    YearOfSaleLeaseStartKey = response.YearOfSaleLeaseStart?.Key,
                    ValueOfInfrastructureAllocatedToLand = response.ValueOfInfrastructureAllocatedToLand,
                    ProjectToLandSaleRelationshipProjectOverviewId = response.ProjectToLandSaleRelationshipProjectOverviewId,
                    ProjectToLandSaleRelationshipProjectOverviewERC = response.ProjectToLandSaleRelationshipProjectOverviewERC
                };

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error in GRTAppService.GetLandSaleByIdAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<GRTLandSaleResponseDto> CreateLandSaleAsync(
            GRTLandSaleDto landSale,
            CancellationToken cancellationToken = default)
        {
            if (landSale == null)
            {
                throw new ArgumentNullException(nameof(landSale), "Land sale cannot be null");
            }

            try
            {
                var request = new GRTLandSaleRequest
                {
                    PlotName = landSale.PlotName,
                    LandUse = BuildKeyValue(landSale.LandUseKey, landSale.LandUseKey),
                    LandType = BuildKeyValue(landSale.LandTypeKey, landSale.LandTypeKey),
                    City = landSale.City,
                    Region = BuildKeyValue(landSale.RegionKey, landSale.RegionKey),
                    RestrictedDevelopmentToSpecificCriteria = BuildKeyValue(landSale.RestrictedDevelopmentToSpecificCriteriaKey, landSale.RestrictedDevelopmentToSpecificCriteriaKey),
                    SaleLease = BuildKeyValue(landSale.SaleLeaseKey, landSale.SaleLeaseKey),
                    NumberOfPlots = landSale.NumberOfPlots,
                    TotalLandArea = landSale.TotalLandArea,
                    AvgSaleLeaseRate = landSale.AvgSaleLeaseRate,
                    YearOfSaleLeaseStart = BuildKeyValue(landSale.YearOfSaleLeaseStartKey, landSale.YearOfSaleLeaseStartKey),
                    ValueOfInfrastructureAllocatedToLand = landSale.ValueOfInfrastructureAllocatedToLand,
                    ProjectToLandSaleRelationshipProjectOverviewId = landSale.ProjectToLandSaleRelationshipProjectOverviewId,
                    ProjectToLandSaleRelationshipProjectOverviewERC = landSale.ProjectToLandSaleRelationshipProjectOverviewERC
                };

                var response = await _grtIntegrationService.CreateLandSaleAsync(request, cancellationToken);

                return new GRTLandSaleResponseDto
                {
                    Id = response.Id,
                    ExternalReferenceCode = response.ExternalReferenceCode,
                    DateCreated = DateTime.TryParse(response.DateCreated, out var dateCreated) ? dateCreated : DateTime.Now,
                    DateModified = DateTime.TryParse(response.DateModified, out var dateModified) ? dateModified : DateTime.Now,
                    Success = true,
                    Message = "Land sale created successfully"
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error in GRTAppService.CreateLandSaleAsync: {ex.Message}");
                return new GRTLandSaleResponseDto
                {
                    Success = false,
                    Message = $"Error creating land sale: {ex.Message}"
                };
            }
        }

        public async Task<GRTLandSaleResponseDto> UpdateLandSaleAsync(
            long id,
            GRTLandSaleDto landSale,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Land sale ID must be greater than zero", nameof(id));
            }

            if (landSale == null)
            {
                throw new ArgumentNullException(nameof(landSale), "Land sale cannot be null");
            }

            try
            {
                var request = new GRTLandSaleRequest
                {
                    PlotName = landSale.PlotName,
                    LandUse = BuildKeyValue(landSale.LandUseKey, landSale.LandUseKey),
                    LandType = BuildKeyValue(landSale.LandTypeKey, landSale.LandTypeKey),
                    City = landSale.City,
                    Region = BuildKeyValue(landSale.RegionKey, landSale.RegionKey),
                    RestrictedDevelopmentToSpecificCriteria = BuildKeyValue(landSale.RestrictedDevelopmentToSpecificCriteriaKey, landSale.RestrictedDevelopmentToSpecificCriteriaKey),
                    SaleLease = BuildKeyValue(landSale.SaleLeaseKey, landSale.SaleLeaseKey),
                    NumberOfPlots = landSale.NumberOfPlots,
                    TotalLandArea = landSale.TotalLandArea,
                    AvgSaleLeaseRate = landSale.AvgSaleLeaseRate,
                    YearOfSaleLeaseStart = BuildKeyValue(landSale.YearOfSaleLeaseStartKey, landSale.YearOfSaleLeaseStartKey),
                    ValueOfInfrastructureAllocatedToLand = landSale.ValueOfInfrastructureAllocatedToLand,
                    ProjectToLandSaleRelationshipProjectOverviewId = landSale.ProjectToLandSaleRelationshipProjectOverviewId,
                    ProjectToLandSaleRelationshipProjectOverviewERC = landSale.ProjectToLandSaleRelationshipProjectOverviewERC
                };

                var response = await _grtIntegrationService.UpdateLandSaleAsync(id, request, cancellationToken);

                return new GRTLandSaleResponseDto
                {
                    Id = response.Id,
                    ExternalReferenceCode = response.ExternalReferenceCode,
                    DateCreated = DateTime.TryParse(response.DateCreated, out var dateCreated) ? dateCreated : DateTime.Now,
                    DateModified = DateTime.TryParse(response.DateModified, out var dateModified) ? dateModified : DateTime.Now,
                    Success = true,
                    Message = "Land sale updated successfully"
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error in GRTAppService.UpdateLandSaleAsync: {ex.Message}");
                return new GRTLandSaleResponseDto
                {
                    Success = false,
                    Message = $"Error updating land sale: {ex.Message}"
                };
            }
        }

        public async Task<bool> DeleteLandSaleAsync(
            long id,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Land sale ID must be greater than zero", nameof(id));
            }

            try
            {
                return await _grtIntegrationService.DeleteLandSaleAsync(id, cancellationToken);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error in GRTAppService.DeleteLandSaleAsync: {ex.Message}");
                throw;
            }
        }
        #endregion

        #region Cashflows
        public async Task<GRTCashflowsPagedDto> GetCashflowsByProjectIdAsync(
            long projectOverviewId,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _grtIntegrationService.GetCashflowsByProjectIdAsync(
                    projectOverviewId,
                    page,
                    pageSize,
                    cancellationToken);

                if (response == null || response.Items == null)
                {
                    return new GRTCashflowsPagedDto
                    {
                        Items = new List<GRTCashflowDto>(),
                        Page = page,
                        PageSize = pageSize,
                        TotalCount = 0,
                        LastPage = 1
                    };
                }

                var result = new GRTCashflowsPagedDto
                {
                    Page = response.Page,
                    PageSize = response.PageSize,
                    TotalCount = response.TotalCount,
                    LastPage = response.LastPage,
                    Items = response.Items.Select(cashflow => new GRTCashflowDto
                    {
                        Id = cashflow.Id,
                        ExternalReferenceCode = cashflow.ExternalReferenceCode,
                        DateCreated = DateTime.TryParse(cashflow.DateCreated, out var dateCreated) ? dateCreated : (DateTime?)null,
                        DateModified = DateTime.TryParse(cashflow.DateModified, out var dateModified) ? dateModified : (DateTime?)null,
                        ProjectOverviewId = cashflow.ProjectToCashflowRelationshipProjectOverviewId,
                        ProjectOverviewERC = cashflow.ProjectToCashflowRelationshipProjectOverviewERC,
                        Summary = cashflow.Summary,
                        Education = cashflow.Education,
                        PrivateSector = cashflow.PrivateSector,
                        LaborAndStaffAccommodation = cashflow.LaborAndStaffAccommodation,
                        Office = cashflow.Office,
                        SocialInfrastructure = cashflow.SocialInfrastructure,
                        SourcesOfFunds = cashflow.SourcesOfFunds,
                        ProjectLevelIRR = cashflow.ProjectLevelIRR,
                        Retail = cashflow.Retail,
                        Healthcare = cashflow.Healthcare,
                        TransportLogisticIndustrial = cashflow.TransportLogisticIndustrial,
                        GeneralInfrastructure = cashflow.GeneralInfrastructure,
                        OtherAssetClasses = cashflow.OtherAssetClasses,
                        Hospitality = cashflow.Hospitality,
                        EntertainmentAndSport = cashflow.EntertainmentAndSport,
                        UsesOfFunds = cashflow.UsesOfFunds,
                        DevcoFinancials = cashflow.DevcoFinancials
                    }).ToList()
                };

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error in GRTAppService.GetCashflowsByProjectIdAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<GRTCashflowDto> GetCashflowByIdAsync(
            long id,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Cashflow ID must be greater than zero", nameof(id));
            }

            try
            {
                var response = await _grtIntegrationService.GetCashflowByIdAsync(
                    id,
                    cancellationToken);

                if (response == null)
                {
                    return null;
                }

                var result = new GRTCashflowDto
                {
                    Id = response.Id,
                    ExternalReferenceCode = response.ExternalReferenceCode,
                    DateCreated = DateTime.TryParse(response.DateCreated, out var dateCreated) ? dateCreated : (DateTime?)null,
                    DateModified = DateTime.TryParse(response.DateModified, out var dateModified) ? dateModified : (DateTime?)null,
                    ProjectOverviewId = response.ProjectToCashflowRelationshipProjectOverviewId,
                    ProjectOverviewERC = response.ProjectToCashflowRelationshipProjectOverviewERC,
                    Summary = response.Summary,
                    Education = response.Education,
                    PrivateSector = response.PrivateSector,
                    LaborAndStaffAccommodation = response.LaborAndStaffAccommodation,
                    Office = response.Office,
                    SocialInfrastructure = response.SocialInfrastructure,
                    SourcesOfFunds = response.SourcesOfFunds,
                    ProjectLevelIRR = response.ProjectLevelIRR,
                    Retail = response.Retail,
                    Healthcare = response.Healthcare,
                    TransportLogisticIndustrial = response.TransportLogisticIndustrial,
                    GeneralInfrastructure = response.GeneralInfrastructure,
                    OtherAssetClasses = response.OtherAssetClasses,
                    Hospitality = response.Hospitality,
                    EntertainmentAndSport = response.EntertainmentAndSport,
                    UsesOfFunds = response.UsesOfFunds,
                    DevcoFinancials = response.DevcoFinancials
                };

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error in GRTAppService.GetCashflowByIdAsync: {ex.Message}");
                throw;
            }
        }
        #endregion

        #region Budgets
        public async Task<GRTCashflowResponseDto> UpdateCashflowAsync(
            long id,
            GRTCashflowDto cashflow,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Cashflow ID must be greater than zero", nameof(id));
            }

            if (cashflow == null)
            {
                throw new ArgumentNullException(nameof(cashflow), "Cashflow cannot be null");
            }

            try
            {
                // Map application DTO to integration request DTO
                var request = new GRTCashflowRequest
                {
                    ProjectToCashflowRelationshipProjectOverviewId = cashflow.ProjectOverviewId,
                    ProjectToCashflowRelationshipProjectOverviewERC = cashflow.ProjectOverviewERC,
                    Summary = cashflow.Summary,
                    Education = cashflow.Education,
                    PrivateSector = cashflow.PrivateSector,
                    LaborAndStaffAccommodation = cashflow.LaborAndStaffAccommodation,
                    Office = cashflow.Office,
                    SocialInfrastructure = cashflow.SocialInfrastructure,
                    SourcesOfFunds = cashflow.SourcesOfFunds,
                    ProjectLevelIRR = cashflow.ProjectLevelIRR,
                    Retail = cashflow.Retail,
                    Healthcare = cashflow.Healthcare,
                    TransportLogisticIndustrial = cashflow.TransportLogisticIndustrial,
                    GeneralInfrastructure = cashflow.GeneralInfrastructure,
                    OtherAssetClasses = cashflow.OtherAssetClasses,
                    Hospitality = cashflow.Hospitality,
                    EntertainmentAndSport = cashflow.EntertainmentAndSport,
                    UsesOfFunds = cashflow.UsesOfFunds,
                    DevcoFinancials = cashflow.DevcoFinancials
                };

                var response = await _grtIntegrationService.UpdateCashflowAsync(id, request, cancellationToken);

                return new GRTCashflowResponseDto
                {
                    Id = response.Id,
                    ExternalReferenceCode = response.ExternalReferenceCode,
                    DateCreated = DateTime.TryParse(response.DateCreated, out var dateCreated) ? dateCreated : DateTime.Now,
                    DateModified = DateTime.TryParse(response.DateModified, out var dateModified) ? dateModified : DateTime.Now,
                    Success = true,
                    Message = "Cashflow updated successfully"
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error in GRTAppService.UpdateCashflowAsync: {ex.Message}");
                return new GRTCashflowResponseDto
                {
                    Success = false,
                    Message = $"Error updating cashflow: {ex.Message}"
                };
            }
        }
        #endregion

        #region LOI & HMA
        public async Task<GRTLOIHMAsPagedDto> GetLOIHMAsByProjectIdAsync(
            long projectOverviewId,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _grtIntegrationService.GetLOIHMAsByProjectIdAsync(
                    projectOverviewId,
                    page,
                    pageSize,
                    cancellationToken);

                if (response == null || response.Items == null)
                {
                    return new GRTLOIHMAsPagedDto
                    {
                        Items = new List<GRTLOIHMADto>(),
                        Page = page,
                        PageSize = pageSize,
                        TotalCount = 0,
                        LastPage = 1
                    };
                }

                var result = new GRTLOIHMAsPagedDto
                {
                    Page = response.Page,
                    PageSize = response.PageSize,
                    TotalCount = response.TotalCount,
                    LastPage = response.LastPage,
                    Items = response.Items.Select(loihma => new GRTLOIHMADto
                    {
                        Id = loihma.Id,
                        ExternalReferenceCode = loihma.ExternalReferenceCode,
                        DateCreated = DateTime.TryParse(loihma.DateCreated, out var dateCreated) ? dateCreated : (DateTime?)null,
                        DateModified = DateTime.TryParse(loihma.DateModified, out var dateModified) ? dateModified : (DateTime?)null,
                        ProjectOverviewId = loihma.ProjectToLOIHMARelationshipProjectOverviewId,
                        ProjectOverviewERC = loihma.ProjectToLOIHMARelationshipProjectOverviewERC,
                        AssetName = loihma.AssetName,
                        Brand = loihma.Brand,
                        City = loihma.City,
                        HMASigned = loihma.HMASigned,
                        HotelOperatorKey = loihma.HotelOperator?.Key,
                        HotelOperatorName = loihma.HotelOperator?.Name,
                        IfHMALOISignedContractDuration = loihma.IfHMALOISignedContractDuration,
                        IfOtherHotelOperatorFillHere = loihma.IfOtherHotelOperatorFillHere,
                        IfOtherOperatingModelFillHere = loihma.IfOtherOperatingModelFillHere,
                        Item = loihma.Item,
                        KeysPerHotel = loihma.KeysPerHotel,
                        Latitude = loihma.Latitude,
                        LOISigned = loihma.LOISigned,
                        Longitude = loihma.Longitude,
                        OperatingModelKey = loihma.OperatingModel?.Key,
                        OperatingModelName = loihma.OperatingModel?.Name,
                        OperatingModelNewKey = loihma.OperatingModelNew?.Key,
                        OperatingModelNewName = loihma.OperatingModelNew?.Name,
                        OperationalYear = loihma.OperationalYear,
                        PositionscaleKey = loihma.Positionscale?.Key,
                        PositionscaleName = loihma.Positionscale?.Name
                    }).ToList()
                };

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error in GRTAppService.GetLOIHMAsByProjectIdAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<GRTLOIHMADto> GetLOIHMAByIdAsync(
            long id,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("LOI & HMA ID must be greater than zero", nameof(id));
            }

            try
            {
                var response = await _grtIntegrationService.GetLOIHMAByIdAsync(id, cancellationToken);

                if (response == null)
                {
                    return null;
                }

                var result = new GRTLOIHMADto
                {
                    Id = response.Id,
                    ExternalReferenceCode = response.ExternalReferenceCode,
                    DateCreated = DateTime.TryParse(response.DateCreated, out var dateCreated) ? dateCreated : (DateTime?)null,
                    DateModified = DateTime.TryParse(response.DateModified, out var dateModified) ? dateModified : (DateTime?)null,
                    ProjectOverviewId = response.ProjectToLOIHMARelationshipProjectOverviewId,
                    ProjectOverviewERC = response.ProjectToLOIHMARelationshipProjectOverviewERC,
                    AssetName = response.AssetName,
                    Brand = response.Brand,
                    City = response.City,
                    HMASigned = response.HMASigned,
                    HotelOperatorKey = response.HotelOperator?.Key,
                    HotelOperatorName = response.HotelOperator?.Name,
                    IfHMALOISignedContractDuration = response.IfHMALOISignedContractDuration,
                    IfOtherHotelOperatorFillHere = response.IfOtherHotelOperatorFillHere,
                    IfOtherOperatingModelFillHere = response.IfOtherOperatingModelFillHere,
                    Item = response.Item,
                    KeysPerHotel = response.KeysPerHotel,
                    Latitude = response.Latitude,
                    LOISigned = response.LOISigned,
                    Longitude = response.Longitude,
                    OperatingModelKey = response.OperatingModel?.Key,
                    OperatingModelName = response.OperatingModel?.Name,
                    OperatingModelNewKey = response.OperatingModelNew?.Key,
                    OperatingModelNewName = response.OperatingModelNew?.Name,
                    OperationalYear = response.OperationalYear,
                    PositionscaleKey = response.Positionscale?.Key,
                    PositionscaleName = response.Positionscale?.Name
                };

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error in GRTAppService.GetLOIHMAByIdAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<GRTLOIHMAResponseDto> CreateLOIHMAAsync(
            GRTLOIHMADto loihma,
            CancellationToken cancellationToken = default)
        {
            if (loihma == null)
            {
                throw new ArgumentNullException(nameof(loihma), "LOI & HMA cannot be null");
            }

            try
            {
                var request = new GRTLOIHMARequest
                {
                    AssetName = loihma.AssetName,
                    Brand = loihma.Brand,
                    City = loihma.City,
                    HMASigned = loihma.HMASigned,
                    HotelOperator = BuildKeyValue(NormalizeListTypeKey(loihma.HotelOperatorKey), loihma.HotelOperatorName ?? loihma.HotelOperatorKey),
                    IfHMALOISignedContractDuration = loihma.IfHMALOISignedContractDuration,
                    IfOtherHotelOperatorFillHere = loihma.IfOtherHotelOperatorFillHere,
                    IfOtherOperatingModelFillHere = loihma.IfOtherOperatingModelFillHere,
                    Item = loihma.Item,
                    KeysPerHotel = loihma.KeysPerHotel,
                    Latitude = loihma.Latitude,
                    LOISigned = loihma.LOISigned,
                    Longitude = loihma.Longitude,
                    OperatingModel = BuildKeyValue(NormalizeListTypeKey(loihma.OperatingModelKey), loihma.OperatingModelName ?? loihma.OperatingModelKey),
                    OperatingModelNew = BuildKeyValue(NormalizeListTypeKey(loihma.OperatingModelNewKey), loihma.OperatingModelNewName ?? loihma.OperatingModelNewKey),
                    OperationalYear = loihma.OperationalYear,
                    Positionscale = BuildKeyValue(NormalizeListTypeKey(loihma.PositionscaleKey), loihma.PositionscaleName ?? loihma.PositionscaleKey),
                    ProjectToLOIHMARelationshipProjectOverviewId = loihma.ProjectOverviewId,
                    ProjectToLOIHMARelationshipProjectOverviewERC = loihma.ProjectOverviewERC
                };

                var response = await _grtIntegrationService.CreateLOIHMAAsync(request, cancellationToken);

                return new GRTLOIHMAResponseDto
                {
                    Id = response.Id,
                    ExternalReferenceCode = response.ExternalReferenceCode,
                    DateCreated = DateTime.TryParse(response.DateCreated, out var dateCreated) ? dateCreated : DateTime.Now,
                    DateModified = DateTime.TryParse(response.DateModified, out var dateModified) ? dateModified : DateTime.Now,
                    Success = true,
                    Message = "LOI & HMA created successfully"
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error in GRTAppService.CreateLOIHMAAsync: {ex.Message}");
                return new GRTLOIHMAResponseDto
                {
                    Success = false,
                    Message = $"Error creating LOI & HMA: {ex.Message}"
                };
            }
        }

        public async Task<GRTLOIHMAResponseDto> UpdateLOIHMAAsync(
            long projectOverviewId,
            long id,
            GRTLOIHMADto loihma,
            CancellationToken cancellationToken = default)
        {
            if (projectOverviewId <= 0)
            {
                throw new ArgumentException("Project overview ID must be greater than zero", nameof(projectOverviewId));
            }

            if (id <= 0)
            {
                throw new ArgumentException("LOI & HMA ID must be greater than zero", nameof(id));
            }

            if (loihma == null)
            {
                throw new ArgumentNullException(nameof(loihma), "LOI & HMA cannot be null");
            }

            try
            {
                var request = new GRTLOIHMARequest
                {
                    AssetName = loihma.AssetName,
                    Brand = loihma.Brand,
                    City = loihma.City,
                    HMASigned = loihma.HMASigned,

                    HotelOperator = BuildKeyValue(NormalizeListTypeKey(loihma.HotelOperatorKey), loihma.HotelOperatorName ?? loihma.HotelOperatorKey),
                    IfHMALOISignedContractDuration = loihma.IfHMALOISignedContractDuration,
                    IfOtherHotelOperatorFillHere = loihma.IfOtherHotelOperatorFillHere,
                    IfOtherOperatingModelFillHere = loihma.IfOtherOperatingModelFillHere,
                    Item = loihma.Item,
                    KeysPerHotel = loihma.KeysPerHotel,
                    Latitude = loihma.Latitude,
                    LOISigned = loihma.LOISigned,
                    Longitude = loihma.Longitude,
                    OperatingModel = BuildKeyValue(NormalizeListTypeKey(loihma.OperatingModelKey), loihma.OperatingModelName ?? loihma.OperatingModelKey),

                    OperatingModelNew = BuildKeyValue(NormalizeListTypeKey(loihma.OperatingModelNewKey), loihma.OperatingModelNewName ?? loihma.OperatingModelNewKey),

                    OperationalYear = loihma.OperationalYear,
                    Positionscale = BuildKeyValue(NormalizeListTypeKey(loihma.PositionscaleKey), loihma.PositionscaleName ?? loihma.PositionscaleKey),

                    ProjectToLOIHMARelationshipProjectOverviewId = loihma.ProjectOverviewId,
                    ProjectToLOIHMARelationshipProjectOverviewERC = loihma.ProjectOverviewERC
                };

                var response = await _grtIntegrationService.UpdateLOIHMAAsync(projectOverviewId, id, request, cancellationToken);

                return new GRTLOIHMAResponseDto
                {
                    Id = response.Id,
                    ExternalReferenceCode = response.ExternalReferenceCode,
                    DateCreated = DateTime.TryParse(response.DateCreated, out var dateCreated) ? dateCreated : DateTime.Now,
                    DateModified = DateTime.TryParse(response.DateModified, out var dateModified) ? dateModified : DateTime.Now,
                    Success = true,
                    Message = "LOI & HMA updated successfully"
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error in GRTAppService.UpdateLOIHMAAsync: {ex.Message}");
                return new GRTLOIHMAResponseDto
                {
                    Success = false,
                    Message = $"Error updating LOI & HMA: {ex.Message}"
                };
            }
        }

        public async Task<bool> DeleteLOIHMAAsync(
            long id,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("LOI & HMA ID must be greater than zero", nameof(id));
            }

            try
            {
                return await _grtIntegrationService.DeleteLOIHMAAsync(id, cancellationToken);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error in GRTAppService.DeleteLOIHMAAsync: {ex.Message}");
                throw;
            }
        }
        #endregion

        #region Multiple S&U
        public async Task<GRTMultipleSandUsPagedDto> GetMultipleSandUsByProjectIdAsync(
            long projectOverviewId,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _grtIntegrationService.GetMultipleSandUsByProjectIdAsync(
                    projectOverviewId,
                    page,
                    pageSize,
                    cancellationToken);

                if (response == null || response.Items == null)
                {
                    return new GRTMultipleSandUsPagedDto
                    {
                        Items = new List<GRTMultipleSandUListDto>(),
                        Page = page,
                        PageSize = pageSize,
                        TotalCount = 0,
                        LastPage = 1
                    };
                }

                var result = new GRTMultipleSandUsPagedDto
                {
                    Page = response.Page,
                    PageSize = response.PageSize,
                    TotalCount = response.TotalCount,
                    LastPage = response.LastPage,
                    Items = response.Items.Select(item => new GRTMultipleSandUListDto
                    {
                        Id = item.Id,
                        ExternalReferenceCode = item.ExternalReferenceCode,
                        Region = item.Regions?.Name,
                        RegionKey = item.Regions?.Key,
                        CapexTotal = ExtractTotalFromJson(item.CapexJSON),
                        OpexTotal = ExtractTotalFromJson(item.OpexJSON),
                        SourcesTotal = ExtractTotalFromJson(item.TotalSourcesJSON),
                        FinancialsTotal = ExtractTotalFromJson(item.FinancialsSARJSON)
                    }).ToList()
                };

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error in GRTAppService.GetMultipleSandUsByProjectIdAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<GRTMultipleSandUDetailDto> GetMultipleSandUByIdAsync(
            long id,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Multiple S&U ID must be greater than zero", nameof(id));
            }

            try
            {
                var response = await _grtIntegrationService.GetMultipleSandUByIdAsync(id, cancellationToken);

                if (response == null)
                {
                    return null;
                }

                var result = new GRTMultipleSandUDetailDto
                {
                    Id = response.Id,
                    ExternalReferenceCode = response.ExternalReferenceCode,
                    DateCreated = DateTime.TryParse(response.DateCreated, out var dateCreated) ? dateCreated : (DateTime?)null,
                    DateModified = DateTime.TryParse(response.DateModified, out var dateModified) ? dateModified : (DateTime?)null,
                    Region = response.Regions?.Name,
                    RegionKey = response.Regions?.Key,
                    CapexJSON = response.CapexJSON,
                    OpexJSON = response.OpexJSON,
                    TotalSourcesJSON = response.TotalSourcesJSON,
                    FinancialsSARJSON = response.FinancialsSARJSON,
                    ProjectToMultipleSandURelationshipProjectOverviewId = response.ProjectToMultipleSandURelationshipProjectOverviewId,
                    ProjectToMultipleSandURelationshipProjectOverviewERC = response.ProjectToMultipleSandURelationshipProjectOverviewERC
                };

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error in GRTAppService.GetMultipleSandUByIdAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<GRTMultipleSandUResponseDto> CreateMultipleSandUAsync(
            GRTMultipleSandUDto multipleSandU,
            CancellationToken cancellationToken = default)
        {
            if (multipleSandU == null)
            {
                throw new ArgumentNullException(nameof(multipleSandU), "Multiple S&U cannot be null");
            }

            try
            {
                var request = new GRTMultipleSandURequest
                {
                    Regions = BuildKeyValue(multipleSandU.RegionKey, multipleSandU.RegionKey),
                    CapexJSON = multipleSandU.CapexJSON,
                    OpexJSON = multipleSandU.OpexJSON,
                    TotalSourcesJSON = multipleSandU.TotalSourcesJSON,
                    FinancialsSARJSON = multipleSandU.FinancialsSARJSON,
                    ProjectToMultipleSandURelationshipProjectOverviewId = multipleSandU.ProjectToMultipleSandURelationshipProjectOverviewId,
                    ProjectToMultipleSandURelationshipProjectOverviewERC = multipleSandU.ProjectToMultipleSandURelationshipProjectOverviewERC
                };

                var response = await _grtIntegrationService.CreateMultipleSandUAsync(request, cancellationToken);

                return new GRTMultipleSandUResponseDto
                {
                    Id = response.Id,
                    ExternalReferenceCode = response.ExternalReferenceCode,
                    DateCreated = DateTime.TryParse(response.DateCreated, out var dateCreated) ? dateCreated : DateTime.Now,
                    DateModified = DateTime.TryParse(response.DateModified, out var dateModified) ? dateModified : DateTime.Now,
                    Success = true,
                    Message = "Multiple S&U created successfully"
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error in GRTAppService.CreateMultipleSandUAsync: {ex.Message}");
                return new GRTMultipleSandUResponseDto
                {
                    Success = false,
                    Message = $"Error creating Multiple S&U: {ex.Message}"
                };
            }
        }

        public async Task<GRTMultipleSandUResponseDto> UpdateMultipleSandUAsync(
            long projectOverviewId,
            long id,
            GRTMultipleSandUDto multipleSandU,
            CancellationToken cancellationToken = default)
        {
            if (projectOverviewId <= 0)
            {
                throw new ArgumentException("Project overview ID must be greater than zero", nameof(projectOverviewId));
            }

            if (id <= 0)
            {
                throw new ArgumentException("Multiple S&U ID must be greater than zero", nameof(id));
            }

            if (multipleSandU == null)
            {
                throw new ArgumentNullException(nameof(multipleSandU), "Multiple S&U cannot be null");
            }

            try
            {
                var request = new GRTMultipleSandURequest
                {
                    Regions = BuildKeyValue(multipleSandU.RegionKey, multipleSandU.RegionKey),
                    CapexJSON = multipleSandU.CapexJSON,
                    OpexJSON = multipleSandU.OpexJSON,
                    TotalSourcesJSON = multipleSandU.TotalSourcesJSON,
                    FinancialsSARJSON = multipleSandU.FinancialsSARJSON,
                    ProjectToMultipleSandURelationshipProjectOverviewId = multipleSandU.ProjectToMultipleSandURelationshipProjectOverviewId,
                    ProjectToMultipleSandURelationshipProjectOverviewERC = multipleSandU.ProjectToMultipleSandURelationshipProjectOverviewERC
                };

                var response = await _grtIntegrationService.UpdateMultipleSandUAsync(projectOverviewId, id, request, cancellationToken);

                return new GRTMultipleSandUResponseDto
                {
                    Id = response.Id,
                    ExternalReferenceCode = response.ExternalReferenceCode,
                    DateCreated = DateTime.TryParse(response.DateCreated, out var dateCreated) ? dateCreated : DateTime.Now,
                    DateModified = DateTime.TryParse(response.DateModified, out var dateModified) ? dateModified : DateTime.Now,
                    Success = true,
                    Message = "Multiple S&U updated successfully"
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error in GRTAppService.UpdateMultipleSandUAsync: {ex.Message}");
                return new GRTMultipleSandUResponseDto
                {
                    Success = false,
                    Message = $"Error updating Multiple S&U: {ex.Message}"
                };
            }
        }

        public async Task<bool> DeleteMultipleSandUAsync(
            long projectOverviewId,
            long id,
            CancellationToken cancellationToken = default)
        {
            if (projectOverviewId <= 0)
            {
                throw new ArgumentException("Project overview ID must be greater than zero", nameof(projectOverviewId));
            }

            if (id <= 0)
            {
                throw new ArgumentException("Multiple S&U ID must be greater than zero", nameof(id));
            }

            try
            {
                return await _grtIntegrationService.DeleteMultipleSandUAsync(projectOverviewId, id, cancellationToken);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error in GRTAppService.DeleteMultipleSandUAsync: {ex.Message}");
                throw;
            }
        }
        #endregion

        #region Budget Operations
        public async Task<GRTBudgetsPagedDto> GetGRTBudgetsPagedAsync(long poid, int page = 1, int pageSize = 20, string search = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _grtIntegrationService.GetGRTBudgetsPagedAsync(
                  poid,
                  page,
                  pageSize,
                  search,
                  cancellationToken);

                if (response != null)
                {
                    var items = response.Items ?? new List<GRTBudgetResponse>();
                    return new GRTBudgetsPagedDto
                    {
                        Items = items.Select(budget => new GRTBudgetsSummaryDto
                        {
                            Id = budget.Id,
                            StatusLabel = budget.Status?.Label,
                            StatusCode = budget.Status?.Code,
                            Year = budget.BudgetYear?.Key ?? budget.BudgetYear?.Name,
                            BudgetYearKey = budget.BudgetYear?.Key,
                            BudgetYearName = budget.BudgetYear?.Name,
                            ExternalReferenceCode = budget.ExternalReferenceCode,
                            BudgetApprovalStatusKey = budget.BudgetApprovedByCompanyBoDOrItsDelegation?.Key,
                            BudgetApprovalStatusName = budget.BudgetApprovedByCompanyBoDOrItsDelegation?.Name,
                            ProjectOverviewId = budget.ProjectOverviewId,
                            ProjectOverviewERC = budget.ProjectOverviewERC
                        }).ToList(),
                        LastPage = response.LastPage,
                        Page = response.Page,
                        PageSize = response.PageSize,
                        TotalCount = response.TotalCount
                    };
                }
                else
                {
                    return new GRTBudgetsPagedDto
                    {
                        Items = new List<GRTBudgetsSummaryDto>(),
                        Page = page,
                        PageSize = pageSize,
                        TotalCount = 0,
                        LastPage = 1
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error in GRTAppService.GetGRTBudgetsPagedAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<GRTBudgetResponseDto> GetBudgetByIdAsync(long id, CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Budget ID must be greater than zero", nameof(id));
            }

            try
            {
                var response = await _grtIntegrationService.GetBudgetByIdAsync(id, cancellationToken);
                return MapBudgetResponse(response, "Budget retrieved successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error in GRTAppService.GetBudgetByIdAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<GRTBudgetResponseDto> CreateBudgetAsync(GRTBudgetCreateDto budget, CancellationToken cancellationToken = default)
        {
            ValidateBudgetPayload(budget);
            if (!budget.ProjectOverviewId.HasValue || budget.ProjectOverviewId <= 0)
            {
                throw new ArgumentException(
                    "ProjectOverviewId must be provided when creating a budget",
                    nameof(budget.ProjectOverviewId));
            }
            try
            {
                var request = BuildBudgetRequest(budget);
                var response = await _grtIntegrationService.CreateBudgetAsync(request, cancellationToken);
                var result = MapBudgetResponse(response, "Budget created successfully");

                return result ?? new GRTBudgetResponseDto
                {
                    Success = false,
                    Message = "Budget creation returned no data"
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error in GRTAppService.CreateBudgetAsync: {ex.Message}");
                return new GRTBudgetResponseDto
                {
                    Success = false,
                    Message = $"Error creating budget: {ex.Message}"
                };
            }
        }

        public async Task<GRTBudgetResponseDto> UpdateBudgetAsync(long id, GRTBudgetCreateDto budget, CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Budget ID must be greater than zero", nameof(id));
            }

            ValidateBudgetPayload(budget);

            try
            {
                var request = BuildBudgetRequest(budget);
                var response = await _grtIntegrationService.UpdateBudgetAsync(id, request, cancellationToken);
                var result = MapBudgetResponse(response, "Budget updated successfully");

                return result ?? new GRTBudgetResponseDto
                {
                    Success = false,
                    Message = "Budget update returned no data"
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error in GRTAppService.UpdateBudgetAsync: {ex.Message}");
                return new GRTBudgetResponseDto
                {
                    Success = false,
                    Message = $"Error updating budget: {ex.Message}"
                };
            }
        }

        public async Task<bool> DeleteBudgetAsync(long budgetId, CancellationToken cancellationToken = default)
        {
            if (budgetId <= 0)
            {
                throw new ArgumentException("Budget ID must be greater than zero", nameof(budgetId));
            }

            try
            {
                return await _grtIntegrationService.DeleteBudgetAsync(budgetId, cancellationToken);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error in GRTAppService.DeleteBudgetAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<GRTBudgetResponseDto> UpdateBudgetSectionsAsync(
            string externalReferenceCode,
            GRTBudgetSectionsDto sections,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(externalReferenceCode))
            {
                throw new ArgumentNullException(nameof(externalReferenceCode), "External reference code cannot be null or empty");
            }

            if (sections == null)
            {
                throw new ArgumentNullException(nameof(sections), "Budget sections cannot be null");
            }

            try
            {
                var request = new GRTBudgetRequest();
                ApplyBudgetSectionsDtoToRequest(sections, request);

                var response = await _grtIntegrationService.PatchBudgetByExternalReferenceAsync(
                    externalReferenceCode,
                    request,
                    cancellationToken);

                var result = MapBudgetResponse(response, "Budget sections updated successfully");

                return result ?? new GRTBudgetResponseDto
                {
                    Success = false,
                    Message = "Budget sections update returned no data"
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error in GRTAppService.UpdateBudgetSectionsAsync: {ex.Message}");
                return new GRTBudgetResponseDto
                {
                    Success = false,
                    Message = $"Error updating budget sections: {ex.Message}"
                };
            }
        }

        private static void ValidateBudgetPayload(GRTBudgetCreateDto budget)
        {
            if (budget == null)
            {
                throw new ArgumentNullException(nameof(budget), "Budget data cannot be null");
            }

            if (string.IsNullOrWhiteSpace(budget.BudgetYearKey))
            {
                throw new ArgumentException("Budget year key is required", nameof(budget.BudgetYearKey));
            }

            if (string.IsNullOrWhiteSpace(budget.BudgetApprovalStatusKey))
            {
                throw new ArgumentException("Budget approval status key is required", nameof(budget.BudgetApprovalStatusKey));
            }

            if (!budget.ProjectOverviewId.HasValue && string.IsNullOrWhiteSpace(budget.ProjectOverviewERC))
            {
                throw new ArgumentException("Either ProjectOverviewId or ProjectOverviewERC must be provided", nameof(budget.ProjectOverviewId));
            }
        }

        private static GRTBudgetResponseDto MapBudgetResponse(GRTBudgetResponse response, string successMessage = null)
        {
            if (response == null)
            {
                return null;
            }

            return new GRTBudgetResponseDto
            {
                Id = response.Id,
                ExternalReferenceCode = response.ExternalReferenceCode,
                DateCreated = ParseNullableDate(response.DateCreated),
                DateModified = ParseNullableDate(response.DateModified),
                BudgetYearKey = response.BudgetYear?.Key,
                BudgetYearName = response.BudgetYear?.Name,
                BudgetApprovalStatusKey = response.BudgetApprovedByCompanyBoDOrItsDelegation?.Key,
                BudgetApprovalStatusName = response.BudgetApprovedByCompanyBoDOrItsDelegation?.Name,
                StatusLabel = response.Status?.Label,
                ProjectOverviewId = response.ProjectOverviewId,
                ProjectOverviewERC = response.ProjectOverviewERC,
                Sections = MapSectionsFromResponse(response),
                Success = true,
                Message = successMessage
            };
        }

        private static DateTime? ParseNullableDate(string value)
        {
            return DateTime.TryParse(value, out var parsed) ? parsed : (DateTime?)null;
        }

        private static GRTBudgetSectionsDto MapSectionsFromResponse(GRTBudgetResponse response)
        {
            if (response == null)
            {
                return null;
            }

            var varianceMatrix = DeserializeVarianceMatrix(response.Variance);
            var varianceBudgetMatrix = DeserializeBudgetMatrix(response.VarianceBudgetByMonth);
            var cashDepositsMatrix = DeserializeBudgetMatrix(response.CashDeposits);
            var cashDepositsBudgetMatrix = DeserializeBudgetMatrix(response.CashDepositsBudgetByMonth);
            var commitmentsMatrix = DeserializeBudgetMatrix(response.Commitments);
            var commitmentsForecastMatrix = DeserializeBudgetMatrix(response.CommitmentsForecastBudgetByMonth);
            var commitmentActualMatrix = DeserializeBudgetMatrix(response.CommitmentActual);
            var commitmentsActualBudgetMatrix = DeserializeBudgetMatrix(response.CommitmentsActualBudgetByMonth);

            // Keep both shapes populated for maximum compatibility
            if (varianceMatrix == null && varianceBudgetMatrix != null)
            {
                varianceMatrix = ConvertMatrixToVariance(varianceBudgetMatrix);
            }

            if (varianceBudgetMatrix == null && varianceMatrix != null)
            {
                varianceBudgetMatrix = ConvertVarianceToMatrix(varianceMatrix);
            }

            if (cashDepositsMatrix == null)
            {
                cashDepositsMatrix = cashDepositsBudgetMatrix;
            }

            if (cashDepositsBudgetMatrix == null)
            {
                cashDepositsBudgetMatrix = cashDepositsMatrix;
            }

            if (commitmentsMatrix == null)
            {
                commitmentsMatrix = commitmentsForecastMatrix;
            }

            if (commitmentsForecastMatrix == null)
            {
                commitmentsForecastMatrix = commitmentsMatrix;
            }

            if (commitmentActualMatrix == null)
            {
                commitmentActualMatrix = commitmentsActualBudgetMatrix;
            }

            if (commitmentsActualBudgetMatrix == null)
            {
                commitmentsActualBudgetMatrix = commitmentActualMatrix;
            }

            var sections = new GRTBudgetSectionsDto
            {
                ForecastSpendingBudgetByMonth = DeserializeBudgetMatrix(response.ForecastSpendingBudgetByMonth),
                ActualSpendingBudgetByMonth = DeserializeBudgetMatrix(response.ActualSpendingBudgetByMonth),
                VarianceBudgetByMonth = varianceBudgetMatrix,
                Variance = varianceMatrix,
                CashDepositsBudgetByMonth = cashDepositsBudgetMatrix,
                CashDeposits = cashDepositsMatrix,
                CommitmentsForecastBudgetByMonth = commitmentsForecastMatrix,
                Commitments = commitmentsMatrix,
                CommitmentsActualBudgetByMonth = commitmentsActualBudgetMatrix,
                CommitmentActual = commitmentActualMatrix
            };

            return sections;
        }

        private static GRTBudgetRequest BuildBudgetRequest(GRTBudgetCreateDto budget)
        {
            var request = new GRTBudgetRequest
            {
                BudgetYear = BuildKeyValue(budget.BudgetYearKey, budget.BudgetYearName),
                BudgetApprovedByCompanyBoDOrItsDelegation = BuildKeyValue(
                    budget.BudgetApprovalStatusKey,
                    budget.BudgetApprovalStatusName),
                ProjectOverviewId = budget.ProjectOverviewId,
                ProjectOverviewERC = budget.ProjectOverviewERC
            };

            ApplyBudgetSectionsDtoToRequest(budget.Sections, request);

            return request;
        }

        private static void ApplyBudgetSectionsDtoToRequest(GRTBudgetSectionsDto sections, GRTBudgetRequest request)
        {
            if (sections == null || request == null)
            {
                return;
            }

            request.ForecastSpendingBudgetByMonth = SerializeBudgetMatrix(sections.ForecastSpendingBudgetByMonth);
            request.ActualSpendingBudgetByMonth = SerializeBudgetMatrix(sections.ActualSpendingBudgetByMonth);
            request.VarianceBudgetByMonth = SerializeBudgetMatrix(sections.VarianceBudgetByMonth);
            request.Variance = SerializeVarianceMatrix(sections.Variance);
            var cashDepositsBudget = sections.CashDepositsBudgetByMonth ?? sections.CashDeposits;
            var cashDeposits = sections.CashDeposits ?? sections.CashDepositsBudgetByMonth;
            request.CashDepositsBudgetByMonth = SerializeBudgetMatrix(cashDepositsBudget);
            request.CashDeposits = SerializeBudgetMatrix(cashDeposits);
            var commitmentsForecast = sections.CommitmentsForecastBudgetByMonth ?? sections.Commitments;
            var commitments = sections.Commitments ?? sections.CommitmentsForecastBudgetByMonth;
            request.CommitmentsForecastBudgetByMonth = SerializeBudgetMatrix(commitmentsForecast);
            request.Commitments = SerializeBudgetMatrix(commitments);
            var commitmentsActualBudget = sections.CommitmentsActualBudgetByMonth ?? sections.CommitmentActual;
            var commitmentActual = sections.CommitmentActual ?? sections.CommitmentsActualBudgetByMonth;
            request.CommitmentsActualBudgetByMonth = SerializeBudgetMatrix(commitmentsActualBudget);
            request.CommitmentActual = SerializeBudgetMatrix(commitmentActual);
        }

        private static string SerializeBudgetMatrix(BudgetMatrixDto matrix)
        {
            if (matrix == null || matrix.Columns == null || !matrix.Columns.Any())
            {
                return null;
            }

            var payload = new BudgetMatrixPayload
            {
                Columns = matrix.Columns,
                Rows = matrix.Rows ?? new Dictionary<string, List<decimal?>>()
            };

            return JsonConvert.SerializeObject(payload, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Include,
                Formatting = Formatting.None
            });
        }

        private static BudgetMatrixDto DeserializeBudgetMatrix(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            try
            {
                var payload = JsonConvert.DeserializeObject<BudgetMatrixPayload>(json);
                if (payload == null)
                {
                    return null;
                }

                return new BudgetMatrixDto
                {
                    Columns = payload.Columns,
                    Rows = payload.Rows
                };
            }
            catch
            {
                return null;
            }
        }

        private static string SerializeVarianceMatrix(BudgetVarianceMatrixDto matrix)
        {
            if (matrix == null || matrix.Columns == null || !matrix.Columns.Any())
            {
                return null;
            }

            var payload = new BudgetVarianceMatrixPayload
            {
                Columns = matrix.Columns,
                Rows = matrix.Rows ?? new Dictionary<string, Dictionary<string, decimal?>>()
            };

            return JsonConvert.SerializeObject(payload, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Include,
                Formatting = Formatting.None
            });
        }

        private static BudgetVarianceMatrixDto DeserializeVarianceMatrix(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            try
            {
                var payload = JsonConvert.DeserializeObject<BudgetVarianceMatrixPayload>(json);
                if (payload == null)
                {
                    return null;
                }

                return new BudgetVarianceMatrixDto
                {
                    Columns = payload.Columns,
                    Rows = payload.Rows
                };
            }
            catch
            {
                return null;
            }
        }

        private static BudgetVarianceMatrixDto ConvertMatrixToVariance(BudgetMatrixDto matrix)
        {
            if (matrix?.Columns == null || matrix.Rows == null)
            {
                return null;
            }

            var convertedRows = new Dictionary<string, Dictionary<string, decimal?>>();

            foreach (var row in matrix.Rows)
            {
                var values = row.Value ?? new List<decimal?>();
                var labeledValues = new Dictionary<string, decimal?>();

                for (var i = 0; i < matrix.Columns.Count; i++)
                {
                    var column = matrix.Columns[i];
                    var value = i < values.Count ? values[i] : null;
                    labeledValues[column] = value;
                }

                convertedRows[row.Key] = labeledValues;
            }

            return new BudgetVarianceMatrixDto
            {
                Columns = matrix.Columns,
                Rows = convertedRows
            };
        }

        private static BudgetMatrixDto ConvertVarianceToMatrix(BudgetVarianceMatrixDto variance)
        {
            if (variance?.Columns == null || variance.Rows == null)
            {
                return null;
            }

            var convertedRows = new Dictionary<string, List<decimal?>>();

            foreach (var row in variance.Rows)
            {
                var list = new List<decimal?>();
                foreach (var column in variance.Columns)
                {
                    decimal? value = null;

                    if (row.Value != null && row.Value.TryGetValue(column, out var cellValue))
                    {
                        value = cellValue;
                    }

                    list.Add(value);
                }

                convertedRows[row.Key] = list;
            }

            return new BudgetMatrixDto
            {
                Columns = variance.Columns,
                Rows = convertedRows
            };
        }

        private static GRTKeyValue BuildKeyValue(string key, string name)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return null;
            }

            return new GRTKeyValue
            {
                Key = key,
                Name = string.IsNullOrWhiteSpace(name) ? key : name
            };
        }

        /// <summary>
        /// Normalizes list-type keys when callers accidentally send labels (e.g., "HMA") instead of keys.
        /// Liferay list type entry keys are typically lowercase and without spaces/dashes.
        /// </summary>
        private static string NormalizeListTypeKey(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return value
                .Trim()
                .Replace(" ", string.Empty)
                .Replace("-", string.Empty)
                .Replace("_", string.Empty)
                .ToLowerInvariant();
        }

        /// <summary>
        /// Helper method to extract a section total from matrix JSON.
        /// For Multiple S&U payloads, each row's first value is the row total, so the section total
        /// is the sum of the first value of each row.
        /// </summary>
        private double? ExtractTotalFromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            try
            {
                var root = JObject.Parse(json);
                var rowsToken = root["rows"];
                if (rowsToken == null)
                {
                    return null;
                }

                double sum = 0d;
                var foundAny = false;

                // Common shape (your example):
                // { "rows": { "USFNOTCX": [869, 4, 44, ...], ... } }
                if (rowsToken.Type == JTokenType.Object)
                {
                    foreach (var prop in ((JObject)rowsToken).Properties())
                    {
                        var rowValues = prop.Value;
                        if (rowValues == null)
                        {
                            continue;
                        }

                        // Row might be an array directly, or wrapped in an object.
                        if (rowValues.Type == JTokenType.Object)
                        {
                            rowValues = rowValues["values"] ?? rowValues["value"] ?? rowValues["cells"] ?? rowValues;
                        }

                        if (rowValues.Type == JTokenType.Array)
                        {
                            var firstCell = rowValues.ElementAtOrDefault(0);
                            if (TryReadNumber(firstCell, out var rowTotal))
                            {
                                sum += rowTotal;
                                foundAny = true;
                            }
                            else if (firstCell?.Type == JTokenType.Object && TryReadNumber(firstCell["value"], out rowTotal))
                            {
                                sum += rowTotal;
                                foundAny = true;
                            }
                        }
                    }
                }
                // Alternate shape:
                // { "rows": [ { "key": "USFNOTCX", "values": [869,...] }, ... ] }
                else if (rowsToken.Type == JTokenType.Array)
                {
                    foreach (var row in rowsToken.Children())
                    {
                        if (row == null)
                        {
                            continue;
                        }

                        var rowValues = row;
                        if (rowValues.Type == JTokenType.Object)
                        {
                            rowValues = rowValues["values"] ?? rowValues["value"] ?? rowValues["cells"] ?? rowValues["rows"] ?? rowValues;
                        }

                        if (rowValues.Type == JTokenType.Array)
                        {
                            var firstCell = rowValues.ElementAtOrDefault(0);
                            if (TryReadNumber(firstCell, out var rowTotal))
                            {
                                sum += rowTotal;
                                foundAny = true;
                            }
                            else if (firstCell?.Type == JTokenType.Object && TryReadNumber(firstCell["value"], out rowTotal))
                            {
                                sum += rowTotal;
                                foundAny = true;
                            }
                        }
                    }
                }

                return foundAny ? (double?)sum : null;
            }
            catch
            {
                return null;
            }
        }

        private static bool IsTotalRowKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return false;
            }

            var k = key.Trim();

            // English variants
            if (k.Equals("total", StringComparison.OrdinalIgnoreCase) ||
                k.Equals("grand total", StringComparison.OrdinalIgnoreCase) ||
                k.Equals("grandtotal", StringComparison.OrdinalIgnoreCase) ||
                k.Equals("totals", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Common Arabic variants (normalized)
            var normalizedArabic = NormalizeArabic(k);
            return normalizedArabic == "الاجمالي" || normalizedArabic == "اجمالي";
        }

        private static bool TryReadNumber(JToken token, out double value)
        {
            value = 0d;
            if (token == null || token.Type == JTokenType.Null || token.Type == JTokenType.Undefined)
            {
                return false;
            }

            if (token.Type == JTokenType.Integer || token.Type == JTokenType.Float)
            {
                value = token.Value<double>();
                return true;
            }

            if (token.Type == JTokenType.String)
            {
                var s = token.Value<string>();
                if (string.IsNullOrWhiteSpace(s))
                {
                    return false;
                }

                // Remove thousand separators and normalize spaces.
                s = s.Trim().Replace(",", string.Empty);

                return double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out value)
                       || double.TryParse(s, NumberStyles.Any, CultureInfo.CurrentCulture, out value);
            }

            return false;
        }

        private static string NormalizeArabic(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            // Strip whitespace and tatweel, and normalize common Alef forms.
            var sb = new StringBuilder(input.Length);
            foreach (var ch in input)
            {
                if (char.IsWhiteSpace(ch) || ch == 'ـ')
                {
                    continue;
                }

                sb.Append(ch);
            }

            return sb.ToString()
                .Replace('أ', 'ا')
                .Replace('إ', 'ا')
                .Replace('آ', 'ا')
                .Replace("ى", "ي")
                .ToLowerInvariant();
        }
        #endregion

        #region Project Impact
        public async Task<GRTProjectImpactDto> GetProjectImpactByIdAsync(
                            long id,
                            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Project impact ID must be greater than zero", nameof(id));
            }

            try
            {
                var response = await _grtIntegrationService.GetProjectImpactByIdAsync(
                    id,
                    cancellationToken);

                if (response == null)
                {
                    return null;
                }

                var result = new GRTProjectImpactDto
                {
                    Id = response.Id,
                    ExternalReferenceCode = response.ExternalReferenceCode,
                    DateCreated = DateTime.TryParse(response.DateCreated, out var dateCreated) ? dateCreated : (DateTime?)null,
                    DateModified = DateTime.TryParse(response.DateModified, out var dateModified) ? dateModified : (DateTime?)null,

                    ProjectOverviewId = response.ProjectToProjectImpactRelationshipProjectOverviewId,
                    ProjectOverviewERC = response.ProjectToProjectImpactRelationshipProjectOverviewERC,
                    ProjectImpactRelationshipERC = response.ProjectToProjectImpactRelationshipERC,

                    AveragePersonalDisposableIncome = response.AveragePersonalDisposableIncome,
                    EntertainmentSpendHouseholdAnnumSAR = response.EntertainmentSpendHouseholdAnnumSAR,
                    MacroeconomicImpactSection = response.MacroeconomicImpactSection,
                    TotalDomesticOvernightVisits = response.TotalDomesticOvernightVisits,
                    TotalHotelOvernightVisits = response.TotalHotelOvernightVisits,
                    TotalInternationalOvernightVisits = response.TotalInternationalOvernightVisits,
                    TotalNumberOfEmployees = response.TotalNumberOfEmployees,
                    TotalNumberOfHospitalityStaffLabor = response.TotalNumberOfHospitalityStaffLabor,
                    TotalPopulationOfTheProjectSection = response.TotalPopulationOfTheProjectSection
                };

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error in GRTAppService.GetProjectImpactByIdAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<GRTProjectImpactsPagedDto> GetProjectImpactByProjectIdAsync(
          long projectOverviewId,
          int page = 1,
          int pageSize = 20,
          CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _grtIntegrationService.GetProjectImpactByProjectIdAsync(
                    projectOverviewId,
                    page,
                    pageSize,
                    cancellationToken);

                if (response == null || response.Items == null)
                {
                    return new GRTProjectImpactsPagedDto
                    {
                        Items = new List<GRTProjectImpactDto>(),
                        Page = page,
                        PageSize = pageSize,
                        TotalCount = 0,
                        LastPage = 1
                    };
                }

                var result = new GRTProjectImpactsPagedDto
                {
                    Page = response.Page,
                    PageSize = response.PageSize,
                    TotalCount = response.TotalCount,
                    LastPage = response.LastPage,
                    Items = response.Items.Select(impact => new GRTProjectImpactDto
                    {
                        Id = impact.Id,
                        ExternalReferenceCode = impact.ExternalReferenceCode,
                        DateCreated = DateTime.TryParse(impact.DateCreated, out var dateCreated) ? dateCreated : (DateTime?)null,
                        DateModified = DateTime.TryParse(impact.DateModified, out var dateModified) ? dateModified : (DateTime?)null,

                        ProjectOverviewId = impact.ProjectToProjectImpactRelationshipProjectOverviewId,
                        ProjectOverviewERC = impact.ProjectToProjectImpactRelationshipProjectOverviewERC,
                        ProjectImpactRelationshipERC = impact.ProjectToProjectImpactRelationshipERC,

                        AveragePersonalDisposableIncome = impact.AveragePersonalDisposableIncome,
                        EntertainmentSpendHouseholdAnnumSAR = impact.EntertainmentSpendHouseholdAnnumSAR,
                        MacroeconomicImpactSection = impact.MacroeconomicImpactSection,
                        TotalDomesticOvernightVisits = impact.TotalDomesticOvernightVisits,
                        TotalHotelOvernightVisits = impact.TotalHotelOvernightVisits,
                        TotalInternationalOvernightVisits = impact.TotalInternationalOvernightVisits,
                        TotalNumberOfEmployees = impact.TotalNumberOfEmployees,
                        TotalNumberOfHospitalityStaffLabor = impact.TotalNumberOfHospitalityStaffLabor,
                        TotalPopulationOfTheProjectSection = impact.TotalPopulationOfTheProjectSection
                    }).ToList()
                };

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error in GRTAppService.GetProjectImpactByProjectIdAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<GRTProjectImpactResponseDto> UpdateProjectImpactAsync(
            long id,
            long projectOverviewId,
            GRTProjectImpactDto projectImpact,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Project impact ID must be greater than zero", nameof(id));
            }

            if (projectImpact == null)
            {
                throw new ArgumentNullException(nameof(projectImpact), "Project impact cannot be null");
            }

            try
            {
                // Map application DTO to integration request DTO
                var request = new GRTProjectImpactRequest
                {
                    ProjectToProjectImpactRelationshipProjectOverviewId = projectImpact.ProjectOverviewId,
                    ProjectToProjectImpactRelationshipProjectOverviewERC = projectImpact.ProjectOverviewERC,
                    ProjectToProjectImpactRelationshipERC = projectImpact.ProjectImpactRelationshipERC,

                    AveragePersonalDisposableIncome = projectImpact.AveragePersonalDisposableIncome,
                    EntertainmentSpendHouseholdAnnumSAR = projectImpact.EntertainmentSpendHouseholdAnnumSAR,
                    MacroeconomicImpactSection = projectImpact.MacroeconomicImpactSection,
                    TotalDomesticOvernightVisits = projectImpact.TotalDomesticOvernightVisits,
                    TotalHotelOvernightVisits = projectImpact.TotalHotelOvernightVisits,
                    TotalInternationalOvernightVisits = projectImpact.TotalInternationalOvernightVisits,
                    TotalNumberOfEmployees = projectImpact.TotalNumberOfEmployees,
                    TotalNumberOfHospitalityStaffLabor = projectImpact.TotalNumberOfHospitalityStaffLabor,
                    TotalPopulationOfTheProjectSection = projectImpact.TotalPopulationOfTheProjectSection
                };

                var response = await _grtIntegrationService.UpdateProjectImpactAsync(
                    id,
                    projectOverviewId,
                    request,
                    cancellationToken);

                return new GRTProjectImpactResponseDto
                {
                    Id = response.Id,
                    ExternalReferenceCode = response.ExternalReferenceCode,
                    DateCreated = DateTime.TryParse(response.DateCreated, out var dateCreated) ? dateCreated : DateTime.Now,
                    DateModified = DateTime.TryParse(response.DateModified, out var dateModified) ? dateModified : DateTime.Now,
                    Success = true,
                    Message = "Project impact updated successfully"
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error in GRTAppService.UpdateProjectImpactAsync: {ex.Message}");
                return new GRTProjectImpactResponseDto
                {
                    Success = false,
                    Message = $"Error updating project impact: {ex.Message}"
                };
            }
        }


        public async Task<GRTProjectImpactResponseDto> CreateProjectImpactAsync(
                          GRTProjectImpactDto projectImpact,
                          CancellationToken cancellationToken = default)
        {
            if (projectImpact == null)
            {
                throw new ArgumentNullException(nameof(projectImpact), "Project impact cannot be null");
            }

            try
            {
                var request = new GRTProjectImpactRequest
                {
                    ProjectToProjectImpactRelationshipProjectOverviewId = projectImpact.ProjectOverviewId,
                    ProjectToProjectImpactRelationshipProjectOverviewERC = projectImpact.ProjectOverviewERC,
                    ProjectToProjectImpactRelationshipERC = projectImpact.ProjectImpactRelationshipERC,

                    AveragePersonalDisposableIncome = projectImpact.AveragePersonalDisposableIncome,
                    EntertainmentSpendHouseholdAnnumSAR = projectImpact.EntertainmentSpendHouseholdAnnumSAR,
                    MacroeconomicImpactSection = projectImpact.MacroeconomicImpactSection,
                    TotalDomesticOvernightVisits = projectImpact.TotalDomesticOvernightVisits,
                    TotalHotelOvernightVisits = projectImpact.TotalHotelOvernightVisits,
                    TotalInternationalOvernightVisits = projectImpact.TotalInternationalOvernightVisits,
                    TotalNumberOfEmployees = projectImpact.TotalNumberOfEmployees,
                    TotalNumberOfHospitalityStaffLabor = projectImpact.TotalNumberOfHospitalityStaffLabor,
                    TotalPopulationOfTheProjectSection = projectImpact.TotalPopulationOfTheProjectSection
                };

                var response = await _grtIntegrationService.CreateProjectImpactAsync(
                    request,
                    cancellationToken);

                return new GRTProjectImpactResponseDto
                {
                    Id = response.Id,
                    ExternalReferenceCode = response.ExternalReferenceCode,
                    DateCreated = DateTime.TryParse(response.DateCreated, out var dateCreated) ? dateCreated : DateTime.Now,
                    DateModified = DateTime.TryParse(response.DateModified, out var dateModified) ? dateModified : DateTime.Now,
                    Success = true,
                    Message = "Project impact created successfully"
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error in GRTAppService.CreateProjectImpactAsync: {ex.Message}");
                return new GRTProjectImpactResponseDto
                {
                    Success = false,
                    Message = $"Error creating project impact: {ex.Message}"
                };
            }
        }
        #endregion

        #region Approved BP
        public async Task<GRTApprovedBPsPagedDto> GetApprovedBPsByProjectIdAsync(
            long projectOverviewId,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            if (projectOverviewId <= 0)
            {
                throw new ArgumentException("Project overview ID must be greater than zero", nameof(projectOverviewId));
            }

            var response = await _grtIntegrationService.GetApprovedBPsByProjectIdAsync(projectOverviewId, page, pageSize, cancellationToken);

            return new GRTApprovedBPsPagedDto
            {
                Items = response.Items?.Select(item => new GRTApprovedBPListDto
                {
                    Id = item.Id,
                    ExternalReferenceCode = item.ExternalReferenceCode,
                    FirstInfrastructureStartDate = item.FirstInfrastructureStartDate?.Name ?? item.FirstInfrastructureStartSate?.Name,
                    OperationsStartDate = item.OperationsStartDate?.Name,
                    LastYearOfFundingRequired = string.IsNullOrEmpty(item.LastYearOfFundingRequired?.Key) ? null : (int?)int.Parse(item.LastYearOfFundingRequired.Key),
                    PIFDateOfApproval = ParseNullableDate(item.PIFDateOfApproval)
                }).ToList() ?? new List<GRTApprovedBPListDto>(),
                Page = response.Page,
                PageSize = response.PageSize,
                TotalCount = response.TotalCount,
                LastPage = response.LastPage
            };
        }

        public async Task<GRTApprovedBPDetailDto> GetApprovedBPByIdAsync(
            long id,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Approved BP ID must be greater than zero", nameof(id));
            }

            var response = await _grtIntegrationService.GetApprovedBPByIdAsync(id, cancellationToken);

            if (response == null)
            {
                return null;
            }

            return new GRTApprovedBPDetailDto
            {
                Id = response.Id,
                ExternalReferenceCode = response.ExternalReferenceCode,
                DateCreated = ParseNullableDate(response.DateCreated),
                DateModified = ParseNullableDate(response.DateModified),

                // Overview As Per Approved BP
                FirstInfrastructureStartDate = response.FirstInfrastructureStartDate?.Name ?? response.FirstInfrastructureStartSate?.Name,
                FirstInfrastructureStartDateKey = response.FirstInfrastructureStartDate?.Key ?? response.FirstInfrastructureStartSate?.Key,
                LastInfrastructureCompleteDate = response.LastInfrastructureCompleteDate?.Name,
                LastInfrastructureCompleteDateKey = response.LastInfrastructureCompleteDate?.Key,
                FirstVerticalConstructionStartDate = response.FirstVerticalConstructionStartDate?.Name,
                FirstVerticalConstructionStartDateKey = response.FirstVerticalConstructionStartDate?.Key,
                LastVerticalConstructionCompleteDate = response.LastVerticalConstructionCompleteDate?.Name,
                LastVerticalConstructionCompleteDateKey = response.LastVerticalConstructionCompleteDate?.Key,
                OperationsStartDate = response.OperationsStartDate?.Name,
                OperationsStartDateKey = response.OperationsStartDate?.Key,
                LastYearOfFundingRequired = response.LastYearOfFundingRequired?.Name,
                LastYearOfFundingRequiredId = string.IsNullOrEmpty(response.LastYearOfFundingRequired?.Key) ? null : (int?)int.Parse(response.LastYearOfFundingRequired.Key),
                PIFDateOfApproval = ParseNullableDate(response.PIFDateOfApproval),

                // IRR approved by PIF
                ProjectIRR = response.ProjectIRR,
                IRRAfterGovernmentSubsidies = response.IRRAfterGovernmentSubsidies,
                EquityIRR = response.EquityIRR,

                DoesApprovedIRRIncludeLand = response.DoesApprovedIRRIncludeLand?.Name,
                DoesApprovedIRRIncludeLandKey = response.DoesApprovedIRRIncludeLand?.Key,
                DoesApprovedIRRIncludeInfrastructureCost = response.DoesApprovedIRRIncludeInfrastructureCost?.Name,
                DoesApprovedIRRIncludeInfrastructureCostKey = response.DoesApprovedIRRIncludeInfrastructureCost?.Key,
                DoesApprovedIRRIncludeGovernmentSubsidies = response.DoesApprovedIRRIncludeGovernmentSubsidies?.Name,
                DoesApprovedIRRIncludeGovernmentSubsidiesKey = response.DoesApprovedIRRIncludeGovernmentSubsidies?.Key,

                ProjectPaybackYear = response.ProjectPaybackYear?.Name,
                ProjectPaybackYearKey = response.ProjectPaybackYear?.Key,
                ProjectPaybackPeriod = response.ProjectPaybackPeriod,

                // Development Plans
                DevelopmentPlanBy2030 = response.DevelopmentPlanBy2030,
                DevelopmentPlanFullDevelopment = response.DevelopmentPlanFullDevelopment,

                // Sources of Funds
                SourcesOfFunds = response.SourcesOfFunds,

                // Financials
                Financials = response.Financials,

                // Relationship
                ProjectToApprovedBPRelationshipProjectOverviewId = response.ProjectToApprovedBPRelationshipProjectOverviewId,
                ProjectToApprovedBPRelationshipProjectOverviewERC = response.ProjectToApprovedBPRelationshipProjectOverviewERC
            };
        }

        public async Task<GRTApprovedBPResponseDto> CreateApprovedBPAsync(
            GRTApprovedBPDto approvedBP,
            CancellationToken cancellationToken = default)
        {
            if (approvedBP == null)
            {
                throw new ArgumentNullException(nameof(approvedBP), "Approved BP data cannot be null");
            }

            var request = new GRTApprovedBPRequest
            {
                // Overview As Per Approved BP
                FirstInfrastructureStartDate = BuildKeyValue(approvedBP.FirstInfrastructureStartDateKey, null),
                FirstInfrastructureStartSate = BuildKeyValue(approvedBP.FirstInfrastructureStartDateKey, null), // Note: API has both fields
                LastInfrastructureCompleteDate = BuildKeyValue(approvedBP.LastInfrastructureCompleteDateKey, null),
                FirstVerticalConstructionStartDate = BuildKeyValue(approvedBP.FirstVerticalConstructionStartDateKey, null),
                LastVerticalConstructionCompleteDate = BuildKeyValue(approvedBP.LastVerticalConstructionCompleteDateKey, null),
                OperationsStartDate = BuildKeyValue(approvedBP.OperationsStartDateKey, null),
                LastYearOfFundingRequired = approvedBP.LastYearOfFundingRequiredId.HasValue ? BuildKeyValue(approvedBP.LastYearOfFundingRequiredId.Value.ToString(), null) : null,
                PIFDateOfApproval = approvedBP.PIFDateOfApproval?.ToString("yyyy-MM-dd"),

                // IRR approved by PIF
                ProjectIRR = approvedBP.ProjectIRR,
                IRRAfterGovernmentSubsidies = approvedBP.IRRAfterGovernmentSubsidies,
                EquityIRR = approvedBP.EquityIRR,

                DoesApprovedIRRIncludeLand = BuildKeyValue(approvedBP.DoesApprovedIRRIncludeLandKey, null),
                DoesApprovedIRRIncludeInfrastructureCost = BuildKeyValue(approvedBP.DoesApprovedIRRIncludeInfrastructureCostKey, null),
                DoesApprovedIRRIncludeGovernmentSubsidies = BuildKeyValue(approvedBP.DoesApprovedIRRIncludeGovernmentSubsidiesKey, null),

                ProjectPaybackYear = BuildKeyValue(approvedBP.ProjectPaybackYearKey, null),
                ProjectPaybackPeriod = approvedBP.ProjectPaybackPeriod,

                // Development Plans
                DevelopmentPlanBy2030 = approvedBP.DevelopmentPlanBy2030,
                DevelopmentPlanFullDevelopment = approvedBP.DevelopmentPlanFullDevelopment,

                // Sources of Funds
                SourcesOfFunds = approvedBP.SourcesOfFunds,

                // Financials
                Financials = approvedBP.Financials,

                // Relationship
                ProjectToApprovedBPRelationshipProjectOverviewId = approvedBP.ProjectToApprovedBPRelationshipProjectOverviewId,
                ProjectToApprovedBPRelationshipProjectOverviewERC = approvedBP.ProjectToApprovedBPRelationshipProjectOverviewERC
            };

            var response = await _grtIntegrationService.CreateApprovedBPAsync(request, cancellationToken);

            return new GRTApprovedBPResponseDto
            {
                Id = response.Id,
                ExternalReferenceCode = response.ExternalReferenceCode,
                DateCreated = ParseNullableDate(response.DateCreated) ?? DateTime.UtcNow,
                DateModified = ParseNullableDate(response.DateModified) ?? DateTime.UtcNow,
                Success = true,
                Message = "Approved BP created successfully"
            };
        }

        public async Task<GRTApprovedBPResponseDto> UpdateApprovedBPAsync(
            long id,
            GRTApprovedBPDto approvedBP,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Approved BP ID must be greater than zero", nameof(id));
            }

            if (approvedBP == null)
            {
                throw new ArgumentNullException(nameof(approvedBP), "Approved BP data cannot be null");
            }

            var request = new GRTApprovedBPRequest
            {
                // Overview As Per Approved BP
                FirstInfrastructureStartDate = BuildKeyValue(approvedBP.FirstInfrastructureStartDateKey, null),
                FirstInfrastructureStartSate = BuildKeyValue(approvedBP.FirstInfrastructureStartDateKey, null),
                LastInfrastructureCompleteDate = BuildKeyValue(approvedBP.LastInfrastructureCompleteDateKey, null),
                FirstVerticalConstructionStartDate = BuildKeyValue(approvedBP.FirstVerticalConstructionStartDateKey, null),
                LastVerticalConstructionCompleteDate = BuildKeyValue(approvedBP.LastVerticalConstructionCompleteDateKey, null),
                OperationsStartDate = BuildKeyValue(approvedBP.OperationsStartDateKey, null),
                LastYearOfFundingRequired = approvedBP.LastYearOfFundingRequiredId.HasValue ? BuildKeyValue(approvedBP.LastYearOfFundingRequiredId.Value.ToString(), null) : null,
                PIFDateOfApproval = approvedBP.PIFDateOfApproval?.ToString("yyyy-MM-dd"),

                // IRR approved by PIF
                ProjectIRR = approvedBP.ProjectIRR,
                IRRAfterGovernmentSubsidies = approvedBP.IRRAfterGovernmentSubsidies,
                EquityIRR = approvedBP.EquityIRR,

                DoesApprovedIRRIncludeLand = BuildKeyValue(approvedBP.DoesApprovedIRRIncludeLandKey, null),
                DoesApprovedIRRIncludeInfrastructureCost = BuildKeyValue(approvedBP.DoesApprovedIRRIncludeInfrastructureCostKey, null),
                DoesApprovedIRRIncludeGovernmentSubsidies = BuildKeyValue(approvedBP.DoesApprovedIRRIncludeGovernmentSubsidiesKey, null),

                ProjectPaybackYear = BuildKeyValue(approvedBP.ProjectPaybackYearKey, null),
                ProjectPaybackPeriod = approvedBP.ProjectPaybackPeriod,

                // Development Plans
                DevelopmentPlanBy2030 = approvedBP.DevelopmentPlanBy2030,
                DevelopmentPlanFullDevelopment = approvedBP.DevelopmentPlanFullDevelopment,

                // Sources of Funds
                SourcesOfFunds = approvedBP.SourcesOfFunds,

                // Financials
                Financials = approvedBP.Financials,

                // Relationship
                ProjectToApprovedBPRelationshipProjectOverviewId = approvedBP.ProjectToApprovedBPRelationshipProjectOverviewId,
                ProjectToApprovedBPRelationshipProjectOverviewERC = approvedBP.ProjectToApprovedBPRelationshipProjectOverviewERC
            };

            var response = await _grtIntegrationService.UpdateApprovedBPAsync(id, request, cancellationToken);

            return new GRTApprovedBPResponseDto
            {
                Id = response.Id,
                ExternalReferenceCode = response.ExternalReferenceCode,
                DateCreated = ParseNullableDate(response.DateCreated) ?? DateTime.UtcNow,
                DateModified = ParseNullableDate(response.DateModified) ?? DateTime.UtcNow,
                Success = true,
                Message = "Approved BP updated successfully"
            };
        }

        public async Task<bool> DeleteApprovedBPAsync(
            long id,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Approved BP ID must be greater than zero", nameof(id));
            }

            return await _grtIntegrationService.DeleteApprovedBPAsync(id, cancellationToken);
        }
        #endregion
    }
}
