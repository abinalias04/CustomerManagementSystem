using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Entity.Models
{
    public class ApplicationUser : IdentityUser<int>
    {
        [MaxLength(100)]
        public string? FullName { get; set; }

        [MaxLength(15)]
        public string? PhoneNumber { get; set; }

        [MaxLength(250)]
        public string? Address { get; set; }

        public int? StateId { get; set; }
        public State? State { get; set; }

        public int? DistrictId { get; set; }
        public District? District { get; set; }

        public int? PincodeId { get; set; }
        public Pincode? Pincode { get; set; }

        public int? PostOfficeId { get; set; }
        public PostOffice? PostOffice { get; set; }

        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }

        [Required] public UserRole Role { get; set; } = UserRole.Customer;
        [Required] public UserStatus Status { get; set; } = UserStatus.Active;
        [Required] public UserBadge Badge { get; set; } = UserBadge.Bronze;

        [Range(0, double.MaxValue)] public decimal NetSpend { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime DateJoined { get; set; } = DateTime.UtcNow;
        public bool IsProfileCompleted { get; set; } = false;

        public ICollection<Cart> Carts { get; set; } = new List<Cart>();
        public ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
        public ICollection<ReturnRequest> ReturnRequests { get; set; } = new List<ReturnRequest>();
    }
}
