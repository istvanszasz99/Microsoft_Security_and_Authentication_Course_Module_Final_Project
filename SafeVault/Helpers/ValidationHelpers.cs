using System;
using System.Linq;
using System.Collections.Generic;

namespace SafeVault.Helpers
{
    public static class ValidationHelpers
    {
        /// <summary>
        /// Checks if the input string contains only letters, digits, or allowed special characters.
        /// </summary>
        /// <param name="input">The input string to validate.</param>
        /// <param name="allowedSpecialCharacters">A string of additional allowed special characters.</param>
        /// <returns>True if input is valid; otherwise, false.</returns>
        public static bool IsValidInput(string input, string allowedSpecialCharacters = "")
        {
            if (string.IsNullOrEmpty(input))
                return false;

            var validCharacters = allowedSpecialCharacters.ToHashSet();

            return input.All(c => char.IsLetterOrDigit(c) || validCharacters.Contains(c));
        }

        /// <summary>
        /// Checks if the input string is valid for XSS (Cross-Site Scripting) prevention.
        /// </summary>
        /// <param name="input">The input string to validate.</param>
        /// <returns>True if input is valid; otherwise, false.</returns>
        public static bool IsValidXSSInput(string input)
        {
            if (string.IsNullOrEmpty(input))
                return true;

            string lowered = input.ToLower();
            if (lowered.Contains("<script") || lowered.Contains("<iframe"))
                return false;

            return true;
        }

        /// <summary>
        /// Removes potentially harmful content from the input string to prevent XSS (Cross-Site Scripting).
        /// </summary>
        /// <param name="input">The input string to sanitize.</param>
        /// <returns>The sanitized string.</returns>
        public static string RemoveXSSContent(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Remove <script> and <iframe> tags (basic approach)
            string output = input;
            output = System.Text.RegularExpressions.Regex.Replace(output, "<script.*?>.*?</script>", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
            output = System.Text.RegularExpressions.Regex.Replace(output, "<iframe.*?>.*?</iframe>", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
            return output;
        }

        // Place this in your test or main method
        public static void TestXssInput()
        {
            string maliciousInput = "<script>alert('XSS');</script>";
            bool isValid = ValidationHelpers.IsValidXSSInput(maliciousInput);
            Console.WriteLine(isValid ? "XSS Test Failed" : "XSS Test Passed");

            string cleaned = ValidationHelpers.RemoveXSSContent(maliciousInput);
            Console.WriteLine("Cleaned input: " + cleaned);
        }
    }
}