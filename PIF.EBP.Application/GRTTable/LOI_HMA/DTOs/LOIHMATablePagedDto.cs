using System.Collections.Generic;


namespace PIF.EBP.Application.GRTTable.LOI_HMA.DTOs
{
    public class LOIHMATablePagedDto
    {
        public List<LOIHMATableDto> Items { get; set; }

        public int Page { get; set; }

        public int PageSize { get; set; }

        public int TotalCount { get; set; }

        public int LastPage { get; set; }
    }

}
