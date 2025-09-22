using System;
using System.ComponentModel.DataAnnotations;
using System.Net.NetworkInformation;

namespace WebApp.Entity.Models
{
    public class UserOtp
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Email { get; set; }

        [Required, MaxLength(6)]
        public string Otp { get; set; }

        [Required]
        public DateTime ExpiresAt { get; set; }
        public OtpStatus Status { get; set; } = OtpStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}