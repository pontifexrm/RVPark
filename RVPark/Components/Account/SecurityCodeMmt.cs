
using System;
using System.Security.Cryptography;
using System.Text;

namespace RVPark.Components.Account
{
    public class SecurityCodeMmt
    {
        public static string GenerateSecurityCode(string email)
        {
            // Get the current time in seconds since epoch
            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return GenerateSecurityCode(email, currentTime);
        }
        public static string GenerateSecurityCode(string email, long currentTime)
        {
            // Define the validity period in seconds (e.g., 2 hours)
            long validityPeriod = 3600 * 2;

            // Calculate the time step
            long timeStep = currentTime / validityPeriod;

            // Combine email and timeStep to generate a unique input for HMAC
            string key = $"{email}{timeStep}";

            // Create a HMAC object using SHA-256
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
            {
                // Generate the hash
                byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(key));

                // Convert the hash to a hexadecimal string and take the first 8 characters
                string securityCode = BitConverter.ToString(hash).Replace("-", "").Substring(0, 8).ToUpper();

                return securityCode;
            }
        }

        public static bool ValidateSecurityCode(string email, string providedCode)
        {
            // Get the current time in seconds since epoch
            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // Generate the expected security code
            string expectedCode = GenerateSecurityCode(email, currentTime);

            // Validate the provided code against the expected code
            return providedCode.Equals(expectedCode);
        }

    }
}
