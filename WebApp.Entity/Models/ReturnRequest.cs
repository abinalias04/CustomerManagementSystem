using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace WebApp.Entity.Models
{
    public class ReturnRequest
    {
        public int ReturnRequestId { get; set; }

        [Required]
        public int UserId { get; set; }
        public ApplicationUser User { get; set; }

        public DateTime ReturnDate { get; set; } = DateTime.UtcNow;

        [Required]
        public ReturnReason Reason { get; set; }

        [MaxLength(500)]
        public string Comments { get; set; }

        [Required]
        public ReturnStatus Status { get; set; } = ReturnStatus.Pending;

        public int? ApprovedBy { get; set; }
        public ApplicationUser ApprovedByUser { get; set; }
        public DateTime? ApprovedAt { get; set; }

        public int? ReceivedBy { get; set; }
        public ApplicationUser ReceivedByUser { get; set; }
        public DateTime? ReceivedAt { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? RefundAmount { get; set; }

        public ICollection<ReturnItem> ReturnItems { get; set; } = new List<ReturnItem>();

        public bool IsWithinPolicy =>
            ReturnItems.All(item =>
                item.PurchaseItem.Purchase.CreatedAt.AddDays(7) >= DateTime.UtcNow);
    }
}
