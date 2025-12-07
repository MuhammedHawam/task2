using PIF.EBP.Application.Shared.AppRequest;
using System;

namespace PIF.EBP.Application.Requests.DTOs
{
    public class HexaRequestListDto_REQ
    {
        public HexaRequestListDto_REQ()
        {
            if (PagingRequest==null)
            {
                PagingRequest = new PagingRequest();
            }
        }
        public string SearchTerm { get; set; }
        public Guid StatusFilter { get; set; }
        public PagingRequest PagingRequest { get; set; }
    }

}
