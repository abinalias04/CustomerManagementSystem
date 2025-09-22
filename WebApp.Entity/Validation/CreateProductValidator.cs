using FluentValidation;
using WebApp.Entity.Dto;

namespace WebApp.Entity.Validations
{
    public class CreateProductValidator : AbstractValidator<CreateProductDto>
    {
        public CreateProductValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
            RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Stock).GreaterThanOrEqualTo(0);

            RuleFor(x => x.ImageFile)
                .Must(f => f == null || f.Length <= 2 * 1024 * 1024) // 2 MB
                .WithMessage("Image must be under 2MB.")
                .Must(f => f == null || new[] { ".jpg", ".jpeg", ".png" }
                    .Contains(Path.GetExtension(f.FileName).ToLower()))
                .WithMessage("Only JPG/PNG allowed.");
        }
    }

    public class UpdateProductValidator : AbstractValidator<UpdateProductDto>
    {
        public UpdateProductValidator()
        {
            RuleFor(x => x.Name)
                .MaximumLength(150)
                .When(x => x.Name != null);

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0)
                .When(x => x.Price.HasValue);

            RuleFor(x => x.Stock)
                .GreaterThanOrEqualTo(0)
                .When(x => x.Stock.HasValue);

            RuleFor(x => x.ImageFile)
                .Must(f => f == null || f.Length <= 2 * 1024 * 1024)
                .WithMessage("Image must be under 2MB.")
                .Must(f => f == null || new[] { ".jpg", ".jpeg", ".png" }
                    .Contains(Path.GetExtension(f.FileName).ToLower()))
                .WithMessage("Only JPG/PNG allowed.");
        }
    }
}
 

