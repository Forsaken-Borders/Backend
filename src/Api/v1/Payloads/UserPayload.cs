using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ForSakenBorders.Backend.Database;

namespace ForSakenBorders.Backend.Api.v1.Payloads
{
    /// <summary>
    /// The payload the user sends to create or edit a new user.
    /// </summary>
    public class UserPayload
    {
        /// <summary>
        /// Optional parameter. Should only be used when editing a user.
        /// </summary>
        public Guid Id { get; set; } = Guid.Empty;

        /// <summary>
        /// The roles the user has.
        /// </summary>
        [MaxLength(200)]
        public List<Role> Roles { get; set; } = new();

        /// <summary>
        /// The user's email. Never exposed to the API.
        /// </summary>
        [Required, EmailAddress, MaxLength(320)]
        public string Email { get; set; }

        /// <summary>
        /// The user's password. Should never be stored, but hashed instead.
        /// </summary>
        [Required]
        public string Password { get; set; }

        /// <summary>
        /// The user's display name. May be duplicates.
        /// </summary>
        [Required, StringLength(32, MinimumLength = 3)]
        public string Username { get; set; }

        /// <summary>
        /// The user's preferred first name.
        /// </summary>
        [StringLength(32, MinimumLength = 3)]
        public string FirstName { get; set; }

        /// <summary>
        /// The user's preferred last name.
        /// </summary>
        [StringLength(32, MinimumLength = 3)]
        public string LastName { get; set; }

        /// <summary>
        /// Roughly validates the user's email. Does not completely abide by the RFC 3696 standard.
        /// </summary>
        public bool ValidateEmail()
        {
            if (Email is null)
            {
                return false;
            }

            if (Email is not string valueAsString)
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