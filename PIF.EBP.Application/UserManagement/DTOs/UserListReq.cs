using PIF.EBP.Application.Shared.AppRequest;
using System;

namespace PIF.EBP.Application.UserManagement.DTOs
{
    public class UserListReq
    {
        public UserListReq()
        {
            if (PagingRequest==null)
            {
                PagingRequest = new PagingRequest();
            }
        }
        public string SearchTerm { get; set; }
        public bool IsReassignOperation { get; set; }
        public Guid RoleFilter { get; set; }
        public PagingRequest PagingRequest { get; set; }
    }
}
