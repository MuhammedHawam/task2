using PIF.EBP.Application.Shared.AppRequest;

namespace PIF.EBP.Application.MetaData.DTOs
{
    public class GridViewDataReq
    {
        public GridViewDataReq()
        {
            if (PagingRequest==null)
            {
                PagingRequest = new PagingRequest();
            }
        }
        public string GridId { get; set; }
        public string EntityName { get; set; }
        public string RelationshipName { get; set; }
        public string RegardingId { get; set; }
        public string CompanyColumn { get; set; }
        public string ContactColumn { get; set; }
        public PagingRequest PagingRequest { get; set; }
    }
}
