using FluentValidation;
using WebApp.Entity.Dto;

public class ReturnItemRequestDtoValidator : AbstractValidator<ReturnItemRequestDto>
{
    public ReturnItemRequestDtoValidator()
    {
        RuleFor(x => x.PurchaseItemId)
            .GreaterThan(0).WithMessage("PurchaseItemId must be greater than 0");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be at least 1");
    }
}

public class CreateReturnRequestDtoValidator : AbstractValidator<CreateReturnRequestDto>
{
    public CreateReturnRequestDtoValidator()
    {
        RuleFor(x => x.PurchaseId)
            .GreaterThan(0).WithMessage("PurchaseId must be greater than 0");

        RuleFor(x => x.Reason)
            .IsInEnum().WithMessage("Invalid return reason");

        RuleFor(x => x.Comments)
            .MaximumLength(500).WithMessage("Comments cannot exceed 500 characters");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one return item is required");

        RuleForEach(x => x.Items)
            .SetValidator(new ReturnItemRequestDtoValidator());
    }
}

public class CompleteReturnDtoValidator : AbstractValidator<CompleteReturnDto>
{
    public CompleteReturnDtoValidator()
    {
        RuleFor(x => x.ReturnRequestId)
            .GreaterThan(0).WithMessage("ReturnRequestId must be greater than 0");

        
    }
}

public class ReturnQueryParametersValidator : AbstractValidator<ReturnQueryParameters>
{
    public ReturnQueryParametersValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be at least 1");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 50).WithMessage("Page size must be between 1 and 50");

        RuleFor(x => x.SortBy)
            .Must(value => string.IsNullOrEmpty(value) ||
                           new[] { "date", "name" }.Contains(value.ToLower()))
            .WithMessage("SortBy must be 'date' or 'name'");

        RuleFor(x => x.SortOrder)
            .Must(value => string.IsNullOrEmpty(value) ||
                           new[] { "asc", "desc" }.Contains(value.ToLower()))
            .WithMessage("SortOrder must be 'asc' or 'desc'");

        RuleFor(x => x.SearchTerm)
            .MaximumLength(100).WithMessage("Search term too long");

        RuleFor(x => x.Status)
            .IsInEnum().When(x => x.Status != null)
            .WithMessage("Invalid return status");
    }
}
