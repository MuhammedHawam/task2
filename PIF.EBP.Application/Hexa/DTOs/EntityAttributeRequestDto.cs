using PIF.EBP.Application.Shared.AppRequest;

namespace PIF.EBP.Application.Hexa.DTOs
{
    public class EntityAttributeRequestDto
    {
        public EntityAttributeRequestDto()
        {
            if (PagingRequest==null)
            {
                PagingRequest = new PagingRequest();
            }
        }
        public string EntityName { get; set; }
        public string SearchTerm { get; set; }
        public PagingRequest PagingRequest { get; set; }
    }
}
