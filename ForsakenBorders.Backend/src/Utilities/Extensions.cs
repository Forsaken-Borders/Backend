using System;
using System.Linq;
using System.Text;
using Konscious.Security.Cryptography;

namespace ForsakenBorders.Backend.Utilities
{
    /// <summary>
    /// A bunch of static extension methods for various class types and conviences.
    /// </summary>
    public static class Extensions
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

            // See https://datatracker.ietf.org/doc/html/rfc5321#section-4.5.3 for max email length. 64 for local part, 255 for domain part and 1 for @ for a grand total of 320.
            string username = emailAddress.Split('@')[0];
            string hostname = emailAddress.Split('@')[1];

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

            int index = emailAddress.IndexOf('@');

            return
                index > 0 &&
                index != emailAddress.Length - 1 &&
                index == emailAddress.LastIndexOf('@');
        }

        /// <summary>
        /// Creates a Argon2ID hash of the given string, using the userid as associated data.
        /// </summary>
        /// <param name="password">The password to hash.</param>
        /// <param name="passwordSalt">The salt to add to the password.</param>
        /// <returns>A 1024 Argon2ID hash.</returns>
        public static byte[] Argon2idHash(this string password, byte[] passwordSalt)
        {
            Argon2id argon2id = new(Encoding.UTF8.GetBytes(password));
            argon2id.DegreeOfParallelism = 1;
            argon2id.Iterations = 2;
            argon2id.MemorySize = 15729;
            argon2id.Salt = passwordSalt;

            return argon2id.GetBytes(1024);
        }
    }
}