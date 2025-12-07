using PIF.EBP.Application.Shared.AppRequest;

namespace PIF.EBP.Application.KnowledgeHub.DTOs
{
    public class PinnedKnowledgeItemReq
    {
        public PinnedKnowledgeItemReq()
        {
            if (PagingRequest == null)
            {
                PagingRequest = new PagingRequest();
            }
        }
        public string SearchTerm { get; set; }
        public int Type { get; set; }
        public int? Filter { get; set; }
        public PagingRequest PagingRequest { get; set; }
    }
}
