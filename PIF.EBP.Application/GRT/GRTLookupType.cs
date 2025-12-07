using System.ComponentModel;

namespace PIF.EBP.Application.GRT
{
    /// <summary>
    /// Enum for GRT Lookup Types with their external reference codes
    /// </summary>
    public enum GRTLookupType
    {
        /// <summary>
        /// Saudi Arabia Regions Picklist
        /// External Reference Code: 6886ba46-023c-9d25-c379-2985f1b2e381
        /// </summary>
        [Description("6886ba46-023c-9d25-c379-2985f1b2e381")]
        SaudiArabiaRegions = 1,

        // Add more lookup types as needed below
        // Example:
        // [Description("your-external-reference-code-here")]
        // YourLookupType = 2,
    }
}
