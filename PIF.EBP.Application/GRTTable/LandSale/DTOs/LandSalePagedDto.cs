using System.Collections.Generic;


namespace PIF.EBP.Application.GRTTable.LandSale.DTOs
{
    public class LandSalePagedDto
    {
        public List<LandSaleDto> Items { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int LastPage { get; set; }
    }

}
