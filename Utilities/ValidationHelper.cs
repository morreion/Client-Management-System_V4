using System;
using System.Text.RegularExpressions;

namespace Client_Management_System_V4.Utilities
{
    public static class ValidationHelper
    {
        /// <summary>
        /// Validates if the string is a valid email format.
        /// </summary>
        /// <param name="email">The email string to validate.</param>
        /// <returns>True if valid or empty (optional), False if invalid format.</returns>
        public static bool IsValidEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return true; // Allow empty emails if they are not required fields

            try
            {
                // Regex for email validation
                // This is a standard regex for email validation
                string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                
                return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }
    }
}
