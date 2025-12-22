using PIF.EBP.Core.GRT;
using System;
using System.Collections.Generic;

namespace PIF.EBP.Application.GRT
{
    /// <summary>
    /// DTO for GRT LOI & HMA (Approved Business Plan) data
    /// </summary>
    public class GRTLOIHMADto
    {
        public long Id { get; set; }
        public string ExternalReferenceCode { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DateModified { get; set; }
        public long? ProjectOverviewId { get; set; }
        public string ProjectOverviewERC { get; set; }
        
        public string AssetName { get; set; }
        public string Brand { get; set; }
        public string City { get; set; }
        public bool? HMASigned { get; set; }
        public string HotelOperatorKey { get; set; }
        public string HotelOperatorName { get; set; }
        public string IfHMALOISignedContractDuration { get; set; }
        public string IfOtherHotelOperatorFillHere { get; set; }
        public string IfOtherOperatingModelFillHere { get; set; }
        public string Item { get; set; }
        public string KeysPerHotel { get; set; }
        public string Latitude { get; set; }
        public bool? LOISigned { get; set; }
        public string Longitude { get; set; }
        public string OperatingModelKey { get; set; }
        public string OperatingModelName { get; set; }
        public string OperatingModelNewKey { get; set; }
        public string OperatingModelNewName { get; set; }
        public string OperationalYear { get; set; }
        public string PositionscaleKey { get; set; }
        public string PositionscaleName { get; set; }

        public List<GRTAuditEvent> AuditEvents { get; set; }
    }

    /// <summary>
    /// Paginated DTO for GRT LOI & HMA
    /// </summary>
    public class GRTLOIHMAsPagedDto
    {
        public System.Collections.Generic.List<GRTLOIHMADto> Items { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int LastPage { get; set; }
    }

    /// <summary>
    /// Response DTO for GRT LOI & HMA create/update operations
    /// </summary>
    public class GRTLOIHMAResponseDto
    {
        public long Id { get; set; }
        public string ExternalReferenceCode { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
