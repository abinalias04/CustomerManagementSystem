using FluentValidation;

public class PurchaseItemDtoValidator : AbstractValidator<PurchaseItemDto>
{
    public PurchaseItemDtoValidator()
    {
        RuleFor(x => x.PurchaseItemId)
            .GreaterThanOrEqualTo(0); // 0 for new, >0 for existing

        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("ProductId must be greater than 0");

        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(100);

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0).WithMessage("Unit price must be greater than 0");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be at least 1");

        RuleFor(x => x.LineTotal)
            .GreaterThan(0).WithMessage("Line total must be valid");
    }
}

public class PurchaseResultDtoValidator : AbstractValidator<PurchaseResultDto>
{
    public PurchaseResultDtoValidator()
    {
        RuleFor(x => x.PurchaseId)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("UserId is required");

        RuleFor(x => x.NetTotal)
            .GreaterThanOrEqualTo(0).WithMessage("Net total cannot be negative");

        RuleFor(x => x.CreatedAt)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Created date cannot be in the future");

        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("User name is required")
            .MaximumLength(50);

        RuleForEach(x => x.Items)
            .SetValidator(new PurchaseItemDtoValidator()); // validate each item
    }
}

public class PurchaseQueryParametersValidator : AbstractValidator<PurchaseQueryParameters>
{
    public PurchaseQueryParametersValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be at least 1");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 50).WithMessage("Page size must be between 1 and 50");

        RuleFor(x => x.SortBy)
            .Must(value => new[] { "date", "total", "user" }.Contains(value.ToLower()))
            .WithMessage("Invalid sort field. Allowed values: date, total, user");

        RuleFor(x => x.SortOrder)
            .Must(value => new[] { "asc", "desc" }.Contains(value.ToLower()))
            .WithMessage("SortOrder must be 'asc' or 'desc'");
    }
}
