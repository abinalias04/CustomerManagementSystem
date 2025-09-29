//using Microsoft.EntityFrameworkCore;
//using WebApp.Entity.Data;
//using WebApp.Entity.Models;
//using System.Collections.Generic;
//using WebApp.Entity.Dto;

//namespace WebApp.Api.Repositories
//{
//    public class DashboardRepository : IDashboardRepository
//    {
//        private readonly AppDbContext _context;

//        // tweakable thresholds
//        private const int LowStockThreshold = 10;
//        // badge thresholds (example; adjust as business rules)
//        private readonly Dictionary<UserBadge, decimal> BadgeThresholds = new()
//        {
//            { UserBadge.Bronze, 0m },
//            { UserBadge.Silver, 10000m },
//            { UserBadge.Gold, 50000m },
//            { UserBadge.Platinum, 100000m }
//        };

//        public DashboardRepository(AppDbContext context)
//        {
//            _context = context;
//        }

//        public async Task<AdminDashboardDto> GetAdminDashboardAsync()
//        {
//            var now = DateTime.UtcNow;
//            var todayStart = now.Date;
//            var weekStart = now.Date.AddDays(-(int)now.Date.DayOfWeek); // Sunday-based; change if needed
//            var monthStart = new DateTime(now.Year, now.Month, 1);

//            var dto = new AdminDashboardDto();

//            // Users overview
//            dto.TotalCustomers = await _context.Users.CountAsync(u => u.Role == UserRole.Customer);
//            dto.ActiveCustomers = await _context.Users.CountAsync(u => u.Role == UserRole.Customer && u.Status == UserStatus.Active);
//            dto.InactiveCustomers = await _context.Users.CountAsync(u => u.Role == UserRole.Customer && u.Status == UserStatus.Inactive);

//            // Customers by badge
//            var badgeGroups = await _context.Users
//                .Where(u => u.Role == UserRole.Customer)
//                .GroupBy(u => u.Badge)
//                .Select(g => new { Badge = g.Key, Count = g.Count() })
//                .ToListAsync();

//            foreach (var g in badgeGroups)
//                dto.CustomersByBadge[g.Badge.ToString()] = g.Count;

//            // Sales & Purchases
//            dto.TotalPurchases = await _context.Purchases.CountAsync();
//            dto.TotalRevenue = await _context.Purchases.SumAsync(p => (decimal?)p.NetTotal) ?? 0m;
//            dto.PurchasesToday = await _context.Purchases.CountAsync(p => p.CreatedAt >= todayStart);
//            dto.PurchasesThisWeek = await _context.Purchases.CountAsync(p => p.CreatedAt >= weekStart);
//            dto.PurchasesThisMonth = await _context.Purchases.CountAsync(p => p.CreatedAt >= monthStart);

//            // Top 5 customers by spend (from Purchases)
//            var topCustomers = await _context.Purchases
//                .GroupBy(p => new { p.UserId })
//                .Select(g => new { g.Key.UserId, Total = g.Sum(x => x.NetTotal) })
//                .OrderByDescending(x => x.Total)
//                .Take(5)
//                .ToListAsync();

//            // join to user names
//            var userIds = topCustomers.Select(x => x.UserId).ToList();
//            var users = await _context.Users
//                .Where(u => userIds.Contains(u.Id))
//                .Select(u => new { u.Id, u.FullName })
//                .ToListAsync();

//            dto.TopCustomersBySpend = topCustomers
//                .Select(x =>
//                {
//                    var u = users.FirstOrDefault(uu => uu.Id == x.UserId);
//                    return new TopCustomerDto { UserId = x.UserId, FullName = u?.FullName ?? $"User {x.UserId}", TotalSpent = x.Total };
//                })
//                .ToList();

//            // Products
//            dto.TotalProducts = await _context.Products.CountAsync();
//            dto.ActiveProducts = await _context.Products.CountAsync(p => p.IsActive);
//            dto.LowStockProducts = await _context.Products.CountAsync(p => p.Stock < LowStockThreshold);

//            // top selling products by PurchaseItem quantity
//            var topProducts = await _context.PurchaseItems
//                .GroupBy(pi => new { pi.ProductId })
//                .Select(g => new { ProductId = g.Key.ProductId, QuantitySold = g.Sum(x => x.Quantity) })
//                .OrderByDescending(x => x.QuantitySold)
//                .Take(10)
//                .ToListAsync();

//            var productIds = topProducts.Select(x => x.ProductId).ToList();
//            var prods = await _context.Products
//                .Where(p => productIds.Contains(p.ProductId))
//                .Select(p => new { p.ProductId, p.Name })
//                .ToListAsync();

//            dto.TopSellingProducts = topProducts
//                .Select(x =>
//                {
//                    var p = prods.FirstOrDefault(pp => pp.ProductId == x.ProductId);
//                    return new TopProductDto { ProductId = x.ProductId, ProductName = p?.Name ?? $"Product {x.ProductId}", QuantitySold = x.QuantitySold };
//                })
//                .ToList();

//            // Returns
//            dto.TotalReturnRequests = await _context.ReturnRequests.CountAsync();
//            dto.PendingReturns = await _context.ReturnRequests.CountAsync(rr => rr.Status == ReturnStatus.Pending);
//            dto.ApprovedReturns = await _context.ReturnRequests.CountAsync(rr => rr.Status == ReturnStatus.Approved);
//            dto.ReceivedReturns = await _context.ReturnRequests.CountAsync(rr => rr.Status == ReturnStatus.Received);

//            // Refund amount: sum of ReturnItem
//            var totalRefundFromItems = await _context.ReturnItems.SumAsync(ri => (decimal?)ri.RefundLineTotal) ?? 0m;
//            var totalRefundRequests = await _context.ReturnRequests.SumAsync(rr => (decimal?)rr.RefundAmount) ?? 0m;

//            dto.TotalRefunded = totalRefundFromItems > 0 ? totalRefundFromItems : totalRefundRequests;

//            // System health
//            dto.NewCustomersThisMonth = await _context.Users.CountAsync(u => u.Role == UserRole.Customer && u.DateJoined >= monthStart);
//            var totalCustomers = dto.TotalCustomers == 0 ? 1 : dto.TotalCustomers; // avoid div by zero
//            var completedProfiles = await _context.Users.CountAsync(u => u.Role == UserRole.Customer && u.IsProfileCompleted);
//            dto.PercentProfilesCompleted = Math.Round((completedProfiles * 100m) / totalCustomers, 2);

//            return dto;
//        }

//        public async Task<CustomerDashboardDto> GetCustomerDashboardAsync(int userId)
//        {
//            var dto = new CustomerDashboardDto();

//            // Basic totals
//            dto.TotalPurchases = await _context.Purchases
//                .CountAsync(p => p.UserId == userId);
//            var user = await _context.Users
//                .Where(u => u.Id == userId)
//                .Select(u => new { u.Id, u.NetSpend, u.Badge, u.IsProfileCompleted, u.FullName })
//                .FirstOrDefaultAsync();

//            dto.TotalSpent = user?.NetSpend ?? 0m;
//            dto.IsProfileCompleted = user?.IsProfileCompleted ?? false;
//            dto.CurrentBadge = user?.Badge.ToString() ?? UserBadge.Bronze.ToString();

//            // Last purchase date
//            dto.LastPurchaseDate = await _context.Purchases
//                .Where(p => p.UserId == userId)
//                .OrderByDescending(p => p.CreatedAt)
//                .Select(p => (DateTime?)p.CreatedAt)
//                .FirstOrDefaultAsync();

//            // Active cart items: any active (IsDeleted == false) cart items for this user's carts
//            dto.ActiveCartItems = await _context.Carts
//                .Where(c => c.UserId == userId && !c.IsDeleted)
//                .SelectMany(c => c.Items)
//                .CountAsync(ci => !ci.IsDeleted);

//            // Returns for this user
//            dto.TotalReturnRequests = await _context.ReturnRequests
//                .CountAsync(rr => rr.UserId == userId);
//            dto.PendingReturns = await _context.ReturnRequests
//                .CountAsync(rr => rr.UserId == userId && rr.Status == ReturnStatus.Pending);
//            // refunds received: sum of RefundLineTotal for ReturnItems where the request was processed (Approved or Received)
//            var refundsFromItems = await (from ri in _context.ReturnItems
//                                          join rr in _context.ReturnRequests on ri.ReturnRequestId equals rr.ReturnRequestId
//                                          where rr.UserId == userId && rr.Status != ReturnStatus.Pending
//                                          select (decimal?)ri.RefundLineTotal).SumAsync() ?? 0m;
//            var refundsFromRequest = await _context.ReturnRequests
//                                        .Where(rr => rr.UserId == userId && rr.RefundAmount != null)
//                                        .SumAsync(rr => (decimal?)rr.RefundAmount) ?? 0m;

//            dto.RefundsReceived = refundsFromItems > 0 ? refundsFromItems : refundsFromRequest;

//            // Badge progress: find next badge threshold and compute remaining amount
//            var currentBadge = user?.Badge ?? UserBadge.Bronze;
//            var currentSpend = user?.NetSpend ?? 0m;

//            // determine next badge
//            var ordered = BadgeThresholds.OrderBy(kv => kv.Value).ToList();
//            var next = ordered.FirstOrDefault(kv => kv.Key > currentBadge);
//            if (!EqualityComparer<KeyValuePair<UserBadge, decimal>>.Default.Equals(next, default))
//            {
//                dto.NextBadgeTarget = next.Value;
//                dto.AmountNeededForNextBadge = Math.Max(0m, next.Value - currentSpend);
//            }
//            else
//            {
//                // already at top tier
//                dto.NextBadgeTarget = currentSpend;
//                dto.AmountNeededForNextBadge = 0m;
//            }

//            // Profile completion percentage (field-level simple heuristic)
//            dto.ProfileCompletionPercent = await ComputeProfileCompletionPercentageAsync(userId);

//            return dto;
//        }

//        private async Task<decimal> ComputeProfileCompletionPercentageAsync(int userId)
//        {
//            var u = await _context.Users
//                .Where(x => x.Id == userId)
//                .Select(x => new
//                {
//                    x.FullName,
//                    x.PhoneNumber,
//                    x.Address,
//                    x.StateId,
//                    x.DistrictId,
//                    x.PincodeId,
//                    x.PostOfficeId,
//                    x.DateOfBirth,
//                    x.Gender
//                }).FirstOrDefaultAsync();

//            if (u == null) return 0m;

//            int totalFields = 9;
//            int filled = 0;
//            if (!string.IsNullOrWhiteSpace(u.FullName)) filled++;
//            if (!string.IsNullOrWhiteSpace(u.PhoneNumber)) filled++;
//            if (!string.IsNullOrWhiteSpace(u.Address)) filled++;
//            if (u.StateId.HasValue) filled++;
//            if (u.DistrictId.HasValue) filled++;
//            if (u.PincodeId.HasValue) filled++;
//            if (u.PostOfficeId.HasValue) filled++;
//            if (u.DateOfBirth.HasValue) filled++;
//            if (!string.IsNullOrWhiteSpace(u.Gender)) filled++;

//            return Math.Round((filled * 100m) / totalFields, 2);
//        }
//    }
//}



using Microsoft.EntityFrameworkCore;
using WebApp.Entity.Data;
using WebApp.Entity.Dto;
using WebApp.Entity.Models;

namespace WebApp.Api.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly AppDbContext _context;
        private const int LowStockThreshold = 10;

        public DashboardRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<AdminDashboardDto> GetAdminDashboardAsync()
        {
            var now = DateTime.UtcNow;
            var todayStart = now.Date;
            var weekStart = now.Date.AddDays(-(int)now.Date.DayOfWeek); // Sunday-based
            var monthStart = new DateTime(now.Year, now.Month, 1);

            var dto = new AdminDashboardDto();


            // Summary numbers

            dto.TotalCustomers = await _context.Users.CountAsync(u => u.Role == UserRole.Customer);
            dto.TotalProducts = await _context.Products.CountAsync();

            dto.PurchasesToday = await _context.Purchases.CountAsync(p => p.CreatedAt >= todayStart);
            dto.PurchasesThisWeek = await _context.Purchases.CountAsync(p => p.CreatedAt >= weekStart);
            dto.PurchasesThisMonth = await _context.Purchases.CountAsync(p => p.CreatedAt >= monthStart);

            dto.TotalRevenue = await _context.Purchases.SumAsync(p => (decimal?)p.NetTotal) ?? 0m;


            // Customers by Badge (Doughnut)

            var badgeGroups = await _context.Users
                .Where(u => u.Role == UserRole.Customer)
                .GroupBy(u => u.Badge)
                .Select(g => new { Badge = g.Key, Count = g.Count() })
                .ToListAsync();

            foreach (var g in badgeGroups)
                dto.CustomersByBadge[g.Badge.ToString()] = g.Count;


            // Total refunded

            var totalRefundFromItems = await _context.ReturnItems.SumAsync(ri => (decimal?)ri.RefundLineTotal) ?? 0m;
            var totalRefundRequests = await _context.ReturnRequests.SumAsync(rr => (decimal?)rr.RefundAmount) ?? 0m;
            dto.TotalRefunded = totalRefundFromItems > 0 ? totalRefundFromItems : totalRefundRequests;

            // Top 5 customers by spend

            var topCustomers = await _context.Purchases
                .GroupBy(p => p.UserId)
                .Select(g => new { UserId = g.Key, TotalSpent = g.Sum(x => x.NetTotal) })
                .OrderByDescending(x => x.TotalSpent)
                .Take(5)
                .ToListAsync();

            var userIds = topCustomers.Select(x => x.UserId).ToList();
            var users = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, u.FullName })
                .ToListAsync();

            dto.TopCustomersBySpend = topCustomers
                .Select(x =>
                {
                    var user = users.FirstOrDefault(u => u.Id == x.UserId);
                    return new TopCustomerDto
                    {
                        UserId = x.UserId,
                        FullName = user?.FullName ?? $"User {x.UserId}",
                        TotalSpent = x.TotalSpent
                    };
                })
                .ToList();

            // Top 10 products by sold quantity

            var topProducts = await _context.PurchaseItems
                .GroupBy(pi => pi.ProductId)
                .Select(g => new { ProductId = g.Key, QuantitySold = g.Sum(x => x.Quantity) })
                .OrderByDescending(x => x.QuantitySold)
                .Take(10)
                .ToListAsync();

            var productIds = topProducts.Select(x => x.ProductId).ToList();
            var products = await _context.Products
                .Where(p => productIds.Contains(p.ProductId))
                .Select(p => new { p.ProductId, p.Name })
                .ToListAsync();

            dto.TopSellingProducts = topProducts
                .Select(x =>
                {
                    var product = products.FirstOrDefault(p => p.ProductId == x.ProductId);
                    return new TopProductDto
                    {
                        ProductId = x.ProductId,
                        ProductName = product?.Name ?? $"Product {x.ProductId}",
                        QuantitySold = x.QuantitySold
                    };
                })
                .ToList();

            return dto;
        }



        public async Task<CustomerDashboardDto> GetCustomerDashboardAsync(int userId)
        {
            var dto = new CustomerDashboardDto();

            // Basic totals
            dto.TotalPurchases = await _context.Purchases.CountAsync(p => p.UserId == userId);
            var user = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => new { u.Id, u.NetSpend, u.Badge, u.IsProfileCompleted, u.FullName })
                .FirstOrDefaultAsync();

            dto.TotalSpent = user?.NetSpend ?? 0m;
            // dto.IsProfileCompleted = user?.IsProfileCompleted ?? false;
            dto.CurrentBadge = user?.Badge.ToString() ?? UserBadge.Bronze.ToString();

            //// Last purchase date
            //dto.LastPurchaseDate = await _context.Purchases
            //    .Where(p => p.UserId == userId)
            //    .OrderByDescending(p => p.CreatedAt)
            //    .Select(p => (DateTime?)p.CreatedAt)
            //    .FirstOrDefaultAsync();

            // Active cart items: any active (IsDeleted == false) cart items for this user's carts
            dto.ActiveCartItems = await _context.Carts
                .Where(c => c.UserId == userId && !c.IsDeleted)
                .SelectMany(c => c.Items)
                .CountAsync(ci => !ci.IsDeleted);

            // Returns for this user
            dto.TotalReturnRequests = await _context.ReturnRequests.CountAsync(rr => rr.UserId == userId);
            dto.PendingReturns = await _context.ReturnRequests.CountAsync(rr => rr.UserId == userId && rr.Status == ReturnStatus.Pending);
            // refunds received: sum of RefundLineTotal for ReturnItems where the request was processed (Approved or Received)
            var refundsFromItems = await (from ri in _context.ReturnItems
                                          join rr in _context.ReturnRequests on ri.ReturnRequestId equals rr.ReturnRequestId
                                          where rr.UserId == userId && rr.Status != ReturnStatus.Pending
                                          select (decimal?)ri.RefundLineTotal).SumAsync() ?? 0m;
            var refundsFromRequest = await _context.ReturnRequests
                                        .Where(rr => rr.UserId == userId && rr.RefundAmount != null)
                                        .SumAsync(rr => (decimal?)rr.RefundAmount) ?? 0m;

            dto.RefundsReceived = refundsFromItems > 0 ? refundsFromItems : refundsFromRequest;

            // Badge progress: find next badge threshold and compute remaining amount
            var currentBadge = user?.Badge ?? UserBadge.Bronze;
            var currentSpend = user?.NetSpend ?? 0m;

            // determine next badge
            //var ordered = BadgeThresholds.OrderBy(kv => kv.Value).ToList();
            //var next = ordered.FirstOrDefault(kv => kv.Key > currentBadge);
            //if (!EqualityComparer<KeyValuePair<UserBadge, decimal>>.Default.Equals(next, default))
            //{
            //    dto.NextBadgeTarget = next.Value;
            //    dto.AmountNeededForNextBadge = Math.Max(0m, next.Value - currentSpend);
            //}
            //else
            //{
            //    // already at top tier
            //    dto.NextBadgeTarget = currentSpend;
            //    dto.AmountNeededForNextBadge = 0m;
            //}

            // Profile completion percentage (field-level simple heuristic)
            dto.ProfileCompletionPercent = await ComputeProfileCompletionPercentageAsync(userId);

            return dto;
        }

        private async Task<decimal> ComputeProfileCompletionPercentageAsync(int userId)
        {
            var u = await _context.Users
                .Where(x => x.Id == userId)
                .Select(x => new
                {
                    x.FullName,
                    x.PhoneNumber,
                    x.Address,
                    x.StateId,
                    x.DistrictId,
                    x.PincodeId,
                    x.PostOfficeId,
                    x.DateOfBirth,
                    x.Gender
                }).FirstOrDefaultAsync();

            if (u == null) return 0m;

            int totalFields = 9;
            int filled = 0;
            if (!string.IsNullOrWhiteSpace(u.FullName)) filled++;
            if (!string.IsNullOrWhiteSpace(u.PhoneNumber)) filled++;
            if (!string.IsNullOrWhiteSpace(u.Address)) filled++;
            if (u.StateId.HasValue) filled++;
            if (u.DistrictId.HasValue) filled++;
            if (u.PincodeId.HasValue) filled++;
            if (u.PostOfficeId.HasValue) filled++;
            if (u.DateOfBirth.HasValue) filled++;
            if (!string.IsNullOrWhiteSpace(u.Gender)) filled++;

            return Math.Round((filled * 100m) / totalFields, 2);
        }
    }
}