using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIF.EBP.Core.GRTTable.LOI.DTOs
{
    public class LOIHMATablePagedResponse
    {
        public List<LOIHMATableItemResponse> Items { get; set; }

        public int Page { get; set; }

        public int PageSize { get; set; }

        public int TotalCount { get; set; }

        public int LastPage { get; set; }
    }

    public class LOIHMATableItemResponse
    {
        public long Id { get; set; }

        public string ExternalReferenceCode { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime DateModified { get; set; }

        /// <summary>
        /// JSON string stored in Liferay object
        /// </summary>
        public string GRTLOITable { get; set; }

        public string ProjectToLOIHMATableRelationshipERC { get; set; }

        public long ProjectToLOIHMATableRelationshipProjectOverviewId { get; set; }

        public string ProjectToLOIHMATableRelationshipProjectOverviewERC { get; set; }
    }


    public class LOIHMATableOperationResponse
    {
        public long Id { get; set; }

        public string ExternalReferenceCode { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime DateModified { get; set; }

        public bool Success { get; set; }

        public string Message { get; set; }
    }



    public class LOIHMATableItemRequest
    {

        public string ExternalReferenceCode { get; set; }

        public string GRTLOITable { get; set; }

        public long ProjectToLOIHMATableRelationshipProjectOverviewId { get; set; }

    }

}
