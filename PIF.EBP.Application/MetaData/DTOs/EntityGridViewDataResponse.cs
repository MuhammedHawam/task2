using PIF.EBP.Application.Shared.AppResponse;
using System.Collections.Generic;

namespace PIF.EBP.Application.MetaData.DTOs
{
    public class EntityGridViewDataResponse: PagingResponse
    {
        public List<EntityGridViewColumn> Columns { get; set; }
        public List<dynamic>  Rows{ get; set; }
    }

    public class EntityGridViewColumn
    {
        public string Key { get; set; }
        public string Name { get; set; }
    }
}
