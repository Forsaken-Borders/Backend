using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ForSakenBorders.Backend.Api.v1.Payloads
{
    public class LoginPayload
    {
        [Required]
        [MaxLength(320)]
        public string Email;

        [Required]
        [StringLength(72, MinimumLength = 8)]
        public string Password;

        public DateTime TokenExpiration = DateTime.UtcNow.AddDays(7);
    }
}