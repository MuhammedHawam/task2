using System;
using System.ComponentModel.DataAnnotations;

namespace PIF.EBP.Application.Db.Model
{
    public class LogTrace
    {
        [Key]
        public int Id { get; set; }
        public string CorrelationId { get; set; }
        public string ChannelCode { get; set; }
        public DateTime RequestDateTime { get; set; }
        public string RequestURI { get; set; }
        public string RequestBody { get; set; }
        public string ResponseCode { get; set; }
        public Nullable<DateTime> ResponseDateTime { get; set; }
        public string ResponseBody { get; set; }
        public string Details { get; set; }
    }
}
