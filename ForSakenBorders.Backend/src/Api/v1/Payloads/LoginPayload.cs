using System;
using System.ComponentModel.DataAnnotations;

namespace ForSakenBorders.Backend.Api.v1.Payloads
{
    public class LoginPayload
    {
        [Required]
        [MaxLength(320)]
        public string Email { get; set; }

        [Required]
        [StringLength(72, MinimumLength = 8)]
        public string Password { get; set; }

        public DateTime TokenExpiration { get; set; } = DateTime.UtcNow.AddDays(7);
    }
}