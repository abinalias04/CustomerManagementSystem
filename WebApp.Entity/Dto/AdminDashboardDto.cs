using WebApp.Entity.Dto;

public class AdminDashboardDto
{
    // Users overview
    public int TotalCustomers { get; set; }
    public int ActiveCustomers { get; set; }
    public int InactiveCustomers { get; set; }
    public IDictionary<string, int> CustomersByBadge { get; set; } = new Dictionary<string, int>();

    // Sales & Purchases
    public int TotalPurchases { get; set; }
    public decimal TotalRevenue { get; set; }
    public int PurchasesToday { get; set; }
    public int PurchasesThisWeek { get; set; }
    public int PurchasesThisMonth { get; set; }
    public List<TopCustomerDto> TopCustomersBySpend { get; set; } = new List<TopCustomerDto>();

    // Products
    public int TotalProducts { get; set; }
    public int ActiveProducts { get; set; }
    public int LowStockProducts { get; set; }
    public List<TopProductDto> TopSellingProducts { get; set; } = new List<TopProductDto>();

    // Returns
    public int TotalReturnRequests { get; set; }
    public int PendingReturns { get; set; }
    public int ApprovedReturns { get; set; }
    public int ReceivedReturns { get; set; }
    public decimal TotalRefunded { get; set; }

    // System health
    public int NewCustomersThisMonth { get; set; }
    public decimal PercentProfilesCompleted { get; set; } // 0..100
}