using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace PIF.EBP.Application.GRT.DTOs
{
    public class GRTBudgetsSummaryDto
    {
        public long Id { get; set; }
        public string Year { get; set; }         // From budgetYear.name 
        public string StatusLabel { get; set; }  // From status.label_i18n
    }

    public class GRTBudgetsPagedDto
    {
        public List<GRTBudgetsSummaryDto> Items { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int LastPage { get; set; }
    }

    /// <summary>
    /// DTO used to create or update a budget record.
    /// </summary>
    public class GRTBudgetCreateDto
    {
        public string BudgetYearKey { get; set; }
        public string BudgetYearName { get; set; }
        public string BudgetApprovalStatusKey { get; set; }
        public string BudgetApprovalStatusName { get; set; }
        public long? ProjectOverviewId { get; set; }
        public string ProjectOverviewERC { get; set; }

        public GRTBudgetSectionsDto Sections { get; set; }
    }

    /// <summary>
    /// Sections that store month by month values for the budget.
    /// </summary>
    public class GRTBudgetSectionsDto
    {
        public BudgetMatrixDto ForecastSpendingBudgetByMonth { get; set; }
        public BudgetMatrixDto ActualSpendingBudgetByMonth { get; set; }
        public BudgetMatrixDto VarianceBudgetByMonth { get; set; }
        public BudgetVarianceMatrixDto Variance { get; set; }
        public BudgetMatrixDto CashDepositsBudgetByMonth { get; set; }
        public BudgetMatrixDto CashDeposits { get; set; }
        public BudgetMatrixDto CommitmentsForecastBudgetByMonth { get; set; }
        public BudgetMatrixDto CommitmentsActualBudgetByMonth { get; set; }
    }

    /// <summary>
    /// Matrix payload with labeled month data (used by variance endpoint).
    /// </summary>
    public class BudgetVarianceMatrixDto
    {
        public List<string> Columns { get; set; }
        public Dictionary<string, Dictionary<string, decimal?>> Rows { get; set; }
    }

    /// <summary>
    /// Matrix payload representing grid values (columns + rows of values).
    /// </summary>
    public class BudgetMatrixDto
    {
        public List<string> Columns { get; set; }
        public Dictionary<string, List<decimal?>> Rows { get; set; }
    }

    /// <summary>
    /// Shape used when serializing/deserializing matrix JSON stored in GRT.
    /// </summary>
    internal class BudgetMatrixPayload
    {
        [JsonProperty("cols")]
        public List<string> Columns { get; set; }

        [JsonProperty("rows")]
        public Dictionary<string, List<decimal?>> Rows { get; set; }
    }

    internal class BudgetVarianceMatrixPayload
    {
        [JsonProperty("cols")]
        public List<string> Columns { get; set; }

        [JsonProperty("rows")]
        public Dictionary<string, Dictionary<string, decimal?>> Rows { get; set; }
    }

    /// <summary>
    /// Standard response returned after any budget mutation.
    /// </summary>
    public class GRTBudgetResponseDto
    {
        public long Id { get; set; }
        public string ExternalReferenceCode { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DateModified { get; set; }
        public string BudgetYearKey { get; set; }
        public string BudgetYearName { get; set; }
        public string BudgetApprovalStatusKey { get; set; }
        public string BudgetApprovalStatusName { get; set; }
        public string StatusLabel { get; set; }
        public long? ProjectOverviewId { get; set; }
        public string ProjectOverviewERC { get; set; }
        public GRTBudgetSectionsDto Sections { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
