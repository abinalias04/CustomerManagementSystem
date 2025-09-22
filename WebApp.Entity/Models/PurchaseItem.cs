using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Entity.Models
{
    public class PurchaseItem
    {
        public int PurchaseItemId { get; set; }

        [Required]
        public int PurchaseId { get; set; }
        public Purchase Purchase { get; set; }

        [Required]
        public int ProductId { get; set; }
        public Product Product { get; set; }

        [Range(0, double.MaxValue)]
        public decimal UnitPriceAtPurchase { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        public decimal LineTotal => UnitPriceAtPurchase * Quantity;

        public ICollection<ReturnItem> ReturnItems { get; set; } = new List<ReturnItem>();
    }
}
