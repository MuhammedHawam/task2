using System;

namespace PIF.EBP.Application.Shared.Helpers
{
    public class OtpHelper
    {
        public static int GenerateOTP(int length)
        {
            var stringOtp = GenerateOTPString(length);

            return Convert.ToInt32(stringOtp);
        }
        private static string GenerateOTPString(int length)
        {
            Random random = new Random();
            string otp = "";

            for (int i = 0; i < length; i++)
            {
                // Generate a random digit from 1 to 9
                int digit = random.Next(1, 10);
                otp += digit.ToString();
            }

            return otp;
        }
    }
}
