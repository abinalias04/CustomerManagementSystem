using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WebApp.Entity.Data;
using WebApp.Entity.Models;

namespace WebApp.Services.Repository
{
    public class PurchaseRepository : IPurchaseRepository
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public PurchaseRepository(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // ------------------ Complete Purchase ------------------
        public async Task<PurchaseResultDto?> CompletePurchaseAsync(int userId)
        {
            var conn = _context.Database.GetDbConnection();
            await using var cmd = conn.CreateCommand();

            cmd.CommandText = "CompletePurchase";
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@UserId", userId));

            if (conn.State == System.Data.ConnectionState.Closed)
                await conn.OpenAsync();

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new PurchaseResultDto
                {
                    PurchaseId = reader.GetInt32(0),
                    UserId = reader.GetInt32(1),
                    NetTotal = reader.GetDecimal(2),
                    CreatedAt = reader.GetDateTime(3),
                    IsProfileCompleted = reader.GetBoolean(4)
                };
            }
            return null;
        }

        // ------------------ Generic Query Builder ------------------
        private IQueryable<Purchase> BuildPurchaseQuery(int? purchaseId = null, int? userId = null, string? sortBy = null, string? sortOrder = null)
        {
            var query = _context.Purchases
                .Include(p => p.User)
                .Include(p => p.PurchaseItems)
                    .ThenInclude(pi => pi.Product)
                .AsQueryable();

            if (purchaseId.HasValue)
                query = query.Where(p => p.PurchaseId == purchaseId.Value);

            if (userId.HasValue)
                query = query.Where(p => p.UserId == userId.Value);

            if (!string.IsNullOrEmpty(sortBy) && !string.IsNullOrEmpty(sortOrder))
            {
                query = sortBy.ToLower() switch
                {
                    "date" => sortOrder.ToLower() == "desc"
                        ? query.OrderByDescending(p => p.CreatedAt)
                        : query.OrderBy(p => p.CreatedAt),
                    _ => query.OrderBy(p => p.CreatedAt)
                };
            }
            else
            {
                query = query.OrderBy(p => p.CreatedAt);
            }

            return query;
        }

        // ------------------ Get All or By ID ------------------
        public async Task<PagedResult<PurchaseResultDto>> GetAllPurchasesAsync(PurchaseQueryParameters parameters, int? purchaseId = null)
        {
            var query = BuildPurchaseQuery(purchaseId: purchaseId, sortBy: parameters.SortBy, sortOrder: parameters.SortOrder);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .Select(p => new PurchaseResultDto
                {
                    PurchaseId = p.PurchaseId,
                    UserId = p.UserId,
                    UserName = p.User.UserName,
                    NetTotal = p.NetTotal,
                    CreatedAt = p.CreatedAt,
                    IsProfileCompleted = p.User.IsProfileCompleted,
                    Items = p.PurchaseItems.Select(pi => new PurchaseItemDto
                    {
                        PurchaseItemId = pi.PurchaseItemId,
                        ProductId = pi.ProductId,
                        ProductName = pi.Product.Name,
                        UnitPrice = pi.UnitPriceAtPurchase,
                        Quantity = pi.Quantity
                    }).ToList()
                })
                .ToListAsync();

            return new PagedResult<PurchaseResultDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize
            };
        }

        // ------------------ Get Purchase By ID ------------------
        public async Task<PurchaseResultDto?> GetPurchaseByIdAsync(int purchaseId)
        {
            var purchase = await BuildPurchaseQuery(purchaseId: purchaseId)
                .FirstOrDefaultAsync();

            if (purchase == null) return null;

            return new PurchaseResultDto
            {
                PurchaseId = purchase.PurchaseId,
                UserId = purchase.UserId,
                UserName = purchase.User.UserName,
                NetTotal = purchase.NetTotal,
                CreatedAt = purchase.CreatedAt,
                IsProfileCompleted = purchase.User.IsProfileCompleted,
                Items = purchase.PurchaseItems.Select(pi => new PurchaseItemDto
                {
                    PurchaseItemId = pi.PurchaseItemId,
                    ProductId = pi.ProductId,
                    ProductName = pi.Product.Name,
                    UnitPrice = pi.UnitPriceAtPurchase,
                    Quantity = pi.Quantity
                }).ToList()
            };
        }

        // ------------------ Get Purchases For Specific User ------------------
        public async Task<PagedResult<PurchaseResultDto>> GetMyPurchasesAsync(int userId, PurchaseQueryParameters parameters)
        {
            var query = BuildPurchaseQuery(userId: userId, sortBy: parameters.SortBy, sortOrder: parameters.SortOrder);

            var totalCount = await query.CountAsync();

            var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .Select(p => new PurchaseResultDto
            {
                PurchaseId = p.PurchaseId,
                UserId = p.UserId,
                NetTotal = p.NetTotal,
                CreatedAt = p.CreatedAt,
                IsProfileCompleted = p.User.IsProfileCompleted,
                Items = p.PurchaseItems.Select(pi => new PurchaseItemDto
                {
                    PurchaseItemId = pi.PurchaseItemId,
                    ProductId = pi.ProductId,
                    ProductName = pi.Product.Name,
                    UnitPrice = pi.Product.Price,
                    Quantity = pi.Quantity,
                    ReturnedQuantity = pi.ReturnItems
                        .Where(ri => ri.ReturnRequest.Status != ReturnStatus.Rejected)
                        .Sum(ri => ri.Quantity)
                }).ToList()
            })
            .ToListAsync(); 



            return new PagedResult<PurchaseResultDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize
            };
        }
    }
}
