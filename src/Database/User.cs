using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using Kiki.Api.v1.Payloads;

namespace Kiki.Database
{
    /// <summary>
    /// A user to be used across all Kiki services.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Creates a new user.
        /// </summary>
        /// <param name="userPayload">Retrieved from the API. See <see cref="UserPayload"/>.</param>
        /// <param name="sha512Generator">Used to calculate the password hash.</param>
        public User(UserPayload userPayload, SHA512 sha512Generator)
        {
            Id = Guid.NewGuid();
            Username = userPayload.Username;
            Email = userPayload.Email;
            PasswordHash = sha512Generator.ComputeHash(Encoding.UTF8.GetBytes(userPayload.Password));
            CreatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// The GUID of the user. Shouldn't ever change, and should be unique.
        /// </summary>
        [Key]
        public Guid Id { get; }

        /// <summary>
        /// Null if the user is not banned, set to the date when the user is supposed to be unbanned.
        /// </summary>
        public DateTime BanExpiration { get; set; }

        /// <summary>
        /// The UTC date when the user was created.
        /// </summary>
        public DateTime CreatedAt { get; } = DateTime.UtcNow;

        /// <summary>
        /// The UTC date when the user was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// If the user is banned.
        /// </summary>
        public bool IsBanned { get; set; }

        /// <summary>
        /// If the user deleted their account.
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// If the user verified their email.
        /// </summary>
        public bool IsVerified { get; set; }

        /// <summary>
        /// The roles the user has.
        /// </summary>
        [MaxLength(200)]
        public List<Role> Roles { get; set; }

        /// <summary>
        /// The reason why the user is banned, if they are banned.
        /// </summary>
        public string BanReason { get; set; }

        /// <summary>
        /// The user's email. Never exposed to the API.
        /// </summary>
        [JsonIgnore]
        [EmailAddress]
        [MaxLength(320)]
        public string Email { get; set; }

        /// <summary>
        /// The user's password hash. Never exposed to the API.
        /// </summary>
        [JsonIgnore]
        public byte[] PasswordHash { get; set; }

        /// <summary>
        /// The user's login token. Used to verify that the user is logged in. Never exposed to the API.
        /// </summary>
        [JsonIgnore]
        public string Token { get; set; }

        /// <summary>
        /// When the user's token expires. Defaults to a week, if "remember me" is checked, set to a month. Client-side configurated. Never exposed to the API.
        /// </summary>
        [JsonIgnore]
        public string TokenExpiration { get; set; }

        /// <summary>
        /// The user's display name. May be duplicates.
        /// </summary>
        [StringLength(32, MinimumLength = 3)]
        public string Username { get; set; }
    }
}