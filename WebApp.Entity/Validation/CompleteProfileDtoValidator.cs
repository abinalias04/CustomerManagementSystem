using FluentValidation;
using WebApp.Entity.Dto;

namespace WebApp.Entity.Validators
{
    public class CompleteProfileDtoValidator : AbstractValidator<CompleteProfileDto>
    {
        public CompleteProfileDtoValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required")
                .MaximumLength(150);
            
            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required")
                .Matches(@"^[6-9]\d{9}$").WithMessage("Invalid Indian phone number format");

            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Address is required")
                .MaximumLength(300);

            RuleFor(x => x.Pincode)
                .NotEmpty().WithMessage("Pincode is required")
                .Matches(@"^\d{6}$").WithMessage("Pincode must be 6 digits");

            RuleFor(x => x.Gender)
                .Must(g => string.IsNullOrEmpty(g) || new[] { "Male", "Female", "Other" }.Contains(g))
                .WithMessage("Gender must be Male, Female, or Other");

            RuleFor(x => x.DateOfBirth)
                .LessThan(DateTime.Today).WithMessage("Date of Birth must be in the past")
                .When(x => x.DateOfBirth.HasValue);
        }
    }

    public class PincodeDetailsDtoValidator : AbstractValidator<PincodeDetailsDto>
    {
        public PincodeDetailsDtoValidator()
        {
            RuleFor(x => x.PincodeId)
                .GreaterThan(0);

            RuleFor(x => x.PincodeValue)
                .NotEmpty().WithMessage("Pincode is required")
                .Matches(@"^\d{6}$").WithMessage("Pincode must be 6 digits");

            RuleFor(x => x.StateId)
                .GreaterThan(0);

            RuleFor(x => x.StateName)
                .NotEmpty().MaximumLength(100);

            RuleFor(x => x.DistrictId)
                .GreaterThan(0);

            RuleFor(x => x.DistrictName)
                .NotEmpty().MaximumLength(100);

            RuleForEach(x => x.PostOffices)
                .SetValidator(new PostOfficeDtoValidator());
        }
    }

    public class PostOfficeDtoValidator : AbstractValidator<PostOfficeDto>
    {
        public PostOfficeDtoValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0);

            RuleFor(x => x.Name)
                .NotEmpty().MaximumLength(150);
        }
    }
}
