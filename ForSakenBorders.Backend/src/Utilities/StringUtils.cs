using System;
using System.Linq;

namespace ForSakenBorders.Backend.Utilities
{
    public static class StringUtils
    {
        /// <summary>
        /// Roughly validates the user's email. Does not completely abide by the RFC 3696 standard.
        /// </summary>
        public static bool ValidateEmail(this string emailAddress)
        {
            if (emailAddress is null)
            {
                return false;
            }

            if (emailAddress is not string valueAsString)
            {
                return false;
            }

            // See https://datatracker.ietf.org/doc/html/rfc5321#section-4.5.3 for max email length. 64 for local part, 255 for domain part and 1 for @ for a grand total of 320.
            string username = valueAsString.Split('@')[0];
            string hostname = valueAsString.Split('@')[1];

            // Validate the host name part of the email address
            // TODO: Does not cover direct ip addresses in the host name
            if (Uri.CheckHostName(hostname) != UriHostNameType.Dns || hostname.Length > 255)
            {
                return false;
            }

            // Check if the username part of the email address doesn't contain any arbitrary characters
            // See https://datatracker.ietf.org/doc/html/rfc3696#section-3 for the list of allowed characters
            // I am not implementing this myself due to all the technicalities of the RFC. Instead I'll just be doing a basic sanity check which does not cover all edge cases.
            else if (!username.All(c => char.IsLetterOrDigit(c) || c == '.' || c == '-' || c == '_') || username.Length > 64)
            {
                return false;
            }

            int index = valueAsString.IndexOf('@');

            return
                index > 0 &&
                index != valueAsString.Length - 1 &&
                index == valueAsString.LastIndexOf('@');
        }
    }
}