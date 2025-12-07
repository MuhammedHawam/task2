using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
