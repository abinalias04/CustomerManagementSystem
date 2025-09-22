using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq.Expressions;
using WebApp.Entity.Data;
using WebApp.Entity.Dto;
using WebApp.Entity.Models;

namespace WebApp.Services.Repository
{
    public class ReturnRepository : IReturnRepository
    {
        private readonly AppDbContext _context;

        public ReturnRepository(AppDbContext context)
        {
            _context = context;
        }

        // 1) Create return request
        public async Task<int> CreateReturnRequestAsync(CreateReturnRequestDto dto, int userId)
        {
            try
            {
                var userIdParam = new SqlParameter("@UserId", SqlDbType.Int) { Value = userId };
                var purchaseIdParam = new SqlParameter("@PurchaseId", SqlDbType.Int) { Value = dto.PurchaseId };
                var reasonParam = new SqlParameter("@Reason", SqlDbType.Int) { Value = (int)dto.Reason };
                var commentsParam = new SqlParameter("@Comments", SqlDbType.NVarChar, 500) { Value = dto.Comments ?? string.Empty };

                // Build table-valued parameter (TVP)
                var dtItems = new DataTable();
                dtItems.Columns.Add("PurchaseItemId", typeof(int));
                dtItems.Columns.Add("Quantity", typeof(int));
                foreach (var item in dto.Items)
                    dtItems.Rows.Add(item.PurchaseItemId, item.Quantity);

                var itemsParam = new SqlParameter("@ReturnItems", SqlDbType.Structured)
                {
                    TypeName = "dbo.ReturnItemType", // SQL User-Defined Table Type
                    Value = dtItems
                };

                var returnIdParam = new SqlParameter("@ReturnRequestId", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC [dbo].[CreateReturnRequest] @UserId, @PurchaseId, @Reason, @Comments, @ReturnItems, @ReturnRequestId OUTPUT",
                    userIdParam, purchaseIdParam, reasonParam, commentsParam, itemsParam, returnIdParam);

                return (int)returnIdParam.Value;
            }
            catch (SqlException ex)
            {
                // Catch RAISERROR from SQL and surface as a friendly error
                throw new ApplicationException(ex.Message);
            }
        }


        // 2) Approve return request
        public async Task<bool> ApproveReturnRequestAsync(int returnRequestId, int adminId)
        {
            var rrIdParam = new SqlParameter("@ReturnRequestId", SqlDbType.Int) { Value = returnRequestId };
            var adminParam = new SqlParameter("@AdminId", SqlDbType.Int) { Value = adminId };
            var resultParam = new SqlParameter("@Result", SqlDbType.Bit) { Direction = ParameterDirection.Output };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC [dbo].[ApproveReturnRequest] @ReturnRequestId, @AdminId, @Result OUTPUT",
                rrIdParam, adminParam, resultParam);

            return (bool)resultParam.Value;
        }

        // 3) Complete return request
        public async Task<ReturnSummaryDto?> CompleteReturnAsync(CompleteReturnDto dto, int adminId)
        {
            var returnIdParam = new SqlParameter("@ReturnRequestId", SqlDbType.Int) { Value = dto.ReturnRequestId };
            var adminParam = new SqlParameter("@AdminId", SqlDbType.Int) { Value = adminId };
            var isGoodParam = new SqlParameter("@IsProductGood", SqlDbType.Bit) { Value = dto.IsProductGood };

            var conn = _context.Database.GetDbConnection();
            if (conn.State == ConnectionState.Closed) await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "[dbo].[CompleteReturn]";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(returnIdParam);
            cmd.Parameters.Add(adminParam);
            cmd.Parameters.Add(isGoodParam);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new ReturnSummaryDto
                {
                    ReturnRequestId = reader.GetInt32(0),
                    UserId = reader.GetInt32(1),
                    Status = ((int)reader.GetInt32(2)).ToString(),
                    RefundAmount = reader.IsDBNull(3) ? 0m : reader.GetDecimal(3),
                    ReceivedAt = reader.IsDBNull(4) ? DateTime.MinValue : reader.GetDateTime(4)
                };
            }

            return null;
        }
        private IQueryable<ReturnRequest> BuildReturnQuery()
        {
            return _context.ReturnRequests
                .Include(r => r.User)
                .Include(r => r.ReturnItems)
                    .ThenInclude(ri => ri.PurchaseItem)
                        .ThenInclude(pi => pi.Product);
        }
        private static Expression<Func<ReturnRequest, ReturnResultDto>> MapToDto()
        {
            return r => new ReturnResultDto
            {
                ReturnRequestId = r.ReturnRequestId,
                UserId = r.UserId,
                UserName = r.User.UserName,
                ReturnDate = r.ReturnDate,
                Status = r.Status.ToString(),
                RefundAmount = r.RefundAmount,
                Reason = (int)r.Reason,
                Items = r.ReturnItems.Select(ri => new ReturnItemDto
                {
                    ProductId = ri.PurchaseItem.ProductId,
                    ProductName = ri.PurchaseItem.Product.Name,
                    UnitPrice = ri.PurchaseItem.Product.Price,
                    Quantity = ri.Quantity
                }).ToList()
            };
        }


        public async Task<PagedResult<ReturnResultDto>> GetAllReturnsAsync(ReturnQueryParameters parameters)
        {
            var query = BuildReturnQuery();

            // Filtering
            if (!string.IsNullOrEmpty(parameters.SearchTerm))
                query = query.Where(r => r.User.UserName.Contains(parameters.SearchTerm));
            if (parameters.Status != null)
                query = query.Where(r => r.Status == parameters.Status);

            // Sorting
            query = parameters.SortBy?.ToLower() switch
            {
                "date" => parameters.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(r => r.ReturnDate)
                    : query.OrderBy(r => r.ReturnDate),
                _ => query.OrderBy(r => r.ReturnDate)
            };

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .Select(MapToDto())
                .ToListAsync();

            return new PagedResult<ReturnResultDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize
            };
        }



        public async Task<ReturnResultDto?> GetReturnByIdAsync(int id)
        {
            return await BuildReturnQuery()
                .Where(r => r.ReturnRequestId == id)
                .Select(MapToDto())
                .FirstOrDefaultAsync();
        }


        public async Task<PagedResult<ReturnResultDto>> GetMyReturnsAsync(int userId, ReturnQueryParameters parameters)
        {
            var query = BuildReturnQuery().Where(r => r.UserId == userId);

            if (!string.IsNullOrEmpty(parameters.SearchTerm))
                query = query.Where(r => r.Status.ToString().Contains(parameters.SearchTerm));

            query = parameters.SortBy?.ToLower() switch
            {
                "date" => parameters.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(r => r.ReturnDate)
                    : query.OrderBy(r => r.ReturnDate),
                _ => query.OrderBy(r => r.ReturnDate)
            };

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .Select(MapToDto())
                .ToListAsync();

            return new PagedResult<ReturnResultDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize
            };
        }
    }
}
