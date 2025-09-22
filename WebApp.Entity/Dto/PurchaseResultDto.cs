
public class PurchaseItemDto
{
    public int PurchaseItemId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal => UnitPrice * Quantity;
}

public class PurchaseResultDto
{
    public int PurchaseId { get; set; }
    
    public int UserId { get; set; }
    public decimal NetTotal { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsProfileCompleted { get; set; }
    public string UserName { get; set; }

    public List<PurchaseItemDto> Items { get; set; } = new();
}


public class PurchaseQueryParameters
{
    private int _pageSize = 10; // default 10

    public int PageNumber { get; set; } = 1;

    public int PageSize
    {
        get => _pageSize;  //return
        set => _pageSize = (value > 50) ? 50 : (value <= 0 ? 10 : value); //set 
        // max 50, default 10 if invalid
    }

    public string SortBy { get; set; } = "date";
    public string SortOrder { get; set; } = "asc";
}


public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

