namespace PIF.EBP.Integrations.FileScanning.Implementation
{
    public class OpswatBaseOptions
    {
        public string Url { get; set; } = string.Empty;
        public string SanitizedUrl { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Timeout { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string RuleName { get; set; } = string.Empty;
    }
}
