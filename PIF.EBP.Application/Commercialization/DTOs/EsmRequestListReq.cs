using PIF.EBP.Application.Shared.AppRequest;

namespace PIF.EBP.Application.Commercialization.DTOs
{
    public class EsmRequestListReq
    {
        public EsmRequestListReq()
        {
            if (PagingRequest==null)
            {
                PagingRequest = new PagingRequest();
            }
        }
        public string Search { get; set; }
        public string StatusFilter { get; set; }
        public PagingRequest PagingRequest { get; set; }
    }
}
