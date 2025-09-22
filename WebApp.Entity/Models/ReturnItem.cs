using System.ComponentModel.DataAnnotations;

namespace WebApp.Entity.Models
{
    public class ReturnItem
    {
        public int ReturnItemId { get; set; }

        [Required]
        public int ReturnRequestId { get; set; }
        public ReturnRequest ReturnRequest { get; set; }

        [Required]
        public int PurchaseItemId { get; set; }
        public PurchaseItem PurchaseItem { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Range(0, double.MaxValue)]
        public decimal RefundLineTotal { get; set; }
    }
}
