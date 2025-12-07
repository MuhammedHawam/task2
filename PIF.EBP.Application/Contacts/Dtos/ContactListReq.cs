using PIF.EBP.Application.Shared.AppRequest;

namespace PIF.EBP.Application.Contacts.Dtos
{
    public class ContactListReq
    {
        public ContactListReq()
        {
            if (PagingRequest==null)
            {
                PagingRequest = new PagingRequest();
            }
        }
        public string SearchTerm { get; set; }
        public PagingRequest PagingRequest { get; set; }
    }
}
