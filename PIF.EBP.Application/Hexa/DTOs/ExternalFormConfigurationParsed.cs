using System.Collections.Generic;

namespace PIF.EBP.Application.Hexa.DTOs
{
    public class ExternalFormConfigurationParsed
    {
        public string DisplayName { get; set; }
        public string LogicalName { get; set; }
        public string Description { get; set; }
        public HashSet<string> Attributes { get; set; }
    }
}
