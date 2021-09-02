using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Kiki.Database
{
    public class User
    {
        [Key]
        public int Id { get; }
        public DateTime BanExpiration { get; set; }
        public DateTime CreatedAt { get; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }
        public bool IsBanned { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsVerified { get; set; }
        public List<Role> Roles { get; set; }
        public string BanReason { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Token { get; set; }
        public string TokenExpiration { get; set; }
        public string Username { get; set; }
    }
}