
namespace WebApp.Entity.Dto
{
    public class TopCustomerDto
    {
        public int UserId { get; set; }
        public string? FullName { get; set; }
        public decimal TotalSpent { get; set; }
    }

    public class TopProductDto
    {
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public int QuantitySold { get; set; }
    }

    // --- Customer Dashboard DTOs ---
    public class CustomerDashboardDto
    {
        // Purchase summary
        public int TotalPurchases { get; set; }
        public decimal TotalSpent { get; set; } // from ApplicationUser.NetSpend
        public DateTime? LastPurchaseDate { get; set; }
        public int ActiveCartItems { get; set; }

        // Returns
        public int TotalReturnRequests { get; set; }
        public int PendingReturns { get; set; }
        public decimal RefundsReceived { get; set; }

        // Badges & loyalty
        public string CurrentBadge { get; set; } = string.Empty;
        public decimal NextBadgeTarget { get; set; } // amount required to reach next tier
        public decimal AmountNeededForNextBadge { get; set; }

        // Profile
        public bool IsProfileCompleted { get; set; }
        public decimal ProfileCompletionPercent { get; set; } // 0..100
    }

}
