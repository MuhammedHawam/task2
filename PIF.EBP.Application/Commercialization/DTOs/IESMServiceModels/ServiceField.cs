using System.Collections.Generic;

namespace PIF.EBP.Application.Commercialization.DTOs.IESMServiceModels
{
    public static class ServiceField
    {
        public static double GetValueOrDefault(this Dictionary<string, double> dict, string key)
        {
            if (dict == null)
                return 0;

            return dict.TryGetValue(key, out double value) ? value : 0;
        }
    }
    public enum ServiceState
    {
        Pending = -5,
        Open = 1,
        WorkInProgress = 2,
        ClosedComplete = 3,
        ClosedIncomplete = 4,
        ClosedSkipped = 7
    }
    public enum ServiceStateMapping
    {
        PendingPIFReview = 1,
        Returned = 2,
        Completed = 3,
        WorkInProgress = 4,
        Rejected = 5,
    }
    public class EsmOptionsDto
    {
        public string Value { get; set; }
        public string Name { get; set; }
        public string NameAr { get; set; }

    }
}
