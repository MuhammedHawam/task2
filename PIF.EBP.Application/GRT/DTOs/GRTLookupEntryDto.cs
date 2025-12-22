using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIF.EBP.Application.GRT
{
    /// <summary>
    /// DTO for GRT lookup entry
    /// </summary>
    public class GRTLookupEntryDto
    {
        public string ExternalReferenceCode { get; set; }
        public int Id { get; set; }
        public string Key { get; set; }
        public string Name { get; set; }
        public string ArSA { get; set; }
        public string EnUS { get; set; }
    }

}
