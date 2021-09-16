using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ForSakenBorders.Backend.Database;
using ForSakenBorders.Backend.Utilities;

namespace ForSakenBorders.Backend.Api.v1.Payloads
{
    /// <summary>
    /// The payload the user sends to create or edit a new user.
    /// </summary>
    public class UserPayload
    {
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
        [StringLength(72, MinimumLength = 8)]
        public string Password { get; set; }

        /// <summary>
        /// The user's display name. May be duplicates.
        /// </summary>
        [Required, StringLength(32, MinimumLength = 3)]
        public string Username { get; set; }

        /// <summary>
        /// The user's preferred first name.
        /// </summary>
        [StringLength(32)]
        public string FirstName { get; set; }

        /// <summary>
        /// The user's preferred last name.
        /// </summary>
        [StringLength(32)]
        public string LastName { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is UserPayload payload)
            {
                return payload.Email == Email
                    && EqualityComparer<List<Role>>.Default.Equals(Roles, payload.Roles)
                    && payload.Password == Password
                    && payload.Username == Username
                    && payload.FirstName == FirstName
                    && payload.LastName == LastName;
            }
            else if (obj is User user)
            {
                bool passwordsMatch = Password.Argon2idHash(user.PasswordSalt).SequenceEqual(user.PasswordHash);

                return user.Email == Email
                    && user.Roles.SequenceEqual(Roles)
                    && passwordsMatch
                    && user.Username == Username
                    && user.FirstName == FirstName
                    && user.LastName == LastName;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Roles, Email, Password, Username, FirstName, LastName);
        }
    }
}