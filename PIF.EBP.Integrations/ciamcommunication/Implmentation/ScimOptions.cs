using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIF.EBP.Integrations.CIAMCommunication.Implmentation
{
    public class ScimOptions
    {
        public string BaseUrl { get; set; }          // e.g. https://ciam-uat.pif.gov.sa/scim2
        public int TimeoutSeconds { get; set; } = 30;
        public string TokenUrl { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
}
