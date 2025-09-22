using WebApp.Entity.Models;

namespace WebApp.Entity.Dto
{
    public class ReturnItemRequestDto
    {
        public int PurchaseItemId { get; set; }
        public int Quantity { get; set; }
    }

    public class CreateReturnRequestDto
    {
        public int PurchaseId { get; set; }
        public ReturnReason Reason { get; set; }
        public string Comments { get; set; }
        public List<ReturnItemRequestDto> Items { get; set; } = new List<ReturnItemRequestDto>();
    }

    public class CompleteReturnDto
    {
        public int ReturnRequestId { get; set; }
        // we will override AdminId on server from JWT claim (safer), but keep this in DTO for compatibility
        public int AdminId { get; set; }
        public bool IsProductGood { get; set; }
    }

    public class ReturnSummaryDto
    {
        public int ReturnRequestId { get; set; }
        public int UserId { get; set; }
        public string Status { get; set; }
        public decimal RefundAmount { get; set; }
        public DateTime ReceivedAt { get; set; }
    }
    public class ReturnQueryParameters
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; } = "date"; // "name" or "date"
        public string? SortOrder { get; set; } = "asc"; // asc / desc
        public string? SearchTerm { get; set; } // optional: filter by user name
        public ReturnStatus? Status { get; set; }
    }

  

    public class ReturnResultDto
    {
        public int ReturnRequestId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public DateTime ReturnDate { get; set; }
        public string Status { get; set; }
        public decimal? RefundAmount { get; set; }
        public int Reason { get; set; } 

        public List<ReturnItemDto> Items { get; set; } = new();
    }
    public class ReturnItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
    }
}
