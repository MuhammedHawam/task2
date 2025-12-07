using System.Collections.Generic;

namespace PIF.EBP.Application.Lookups.DTOs
{
    public class MasterLookup
    {
        public string key { get; set; }
        public List<LookupValue> Values { get; set; } = new List<LookupValue>();
    }
}
