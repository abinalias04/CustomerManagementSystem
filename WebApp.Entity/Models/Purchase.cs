using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Entity.Models
{
    public class Purchase
    {
        public int PurchaseId { get; set; }

        [Required]
        public int UserId { get; set; }
        public ApplicationUser User { get; set; }

        //[Range(0, double.MaxValue)]
        //public decimal GrossTotal { get; set; }

        [Range(0, double.MaxValue)]
        public decimal NetTotal { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<PurchaseItem> PurchaseItems { get; set; } = new List<PurchaseItem>();
    }
}
