using System.Collections.Generic;

namespace PIF.EBP.Application.Shared.AppResponse
{
    public abstract class PagingResponse
    {
        public long TotalCount { get; set; }
    }
    public class ListPagingResponse<T> : PagingResponse
    {
        public ListPagingResponse()
        {
            ListResponse = new List<T>();
        }
        public List<T> ListResponse { get; set; }
    }
}
