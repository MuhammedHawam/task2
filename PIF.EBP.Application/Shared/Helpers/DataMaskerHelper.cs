namespace PIF.EBP.Application.Shared.Helpers
{
    public class DataMaskerHelper
    {
        public static string MaskEmail(string email)
        {
            if (string.IsNullOrEmpty(email)) return email;
            var parts = email.Split('@');
            if (parts.Length != 2) return email; // Not a valid email format
            var namePart = parts[0];
            var domainPart = parts[1];
            var maskedName = namePart.Length <= 1 ? namePart : $"{namePart[0]}***{namePart.Substring(namePart.Length - 1)}";
            return $"{maskedName}@{domainPart}";
        }

        public static string MaskPhone(string phone)
        {
            if (string.IsNullOrEmpty(phone)) return phone;
            return phone.Length <= 4 ? phone : $"{new string('*', phone.Length - 4)}{phone.Substring(phone.Length - 4)}";
        }
    }
}
