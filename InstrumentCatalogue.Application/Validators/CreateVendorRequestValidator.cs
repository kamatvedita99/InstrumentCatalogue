using FluentValidation;
using InstrumentCatalogue.Application.DTOs;

namespace InstrumentCatalogue.Application.Validators
{
    public class CreateVendorRequestValidator : AbstractValidator<CreateVendorRequest>
    {
        public CreateVendorRequestValidator()
        {
            RuleFor(cvr => cvr.Name).NotEmpty()
                                   .MaximumLength(100)
                                   .Matches(@"^[^0-9]*$").WithMessage("Name cannot contain numbers");

            RuleFor(cvr => cvr.ShortCode).NotEmpty()
                                         .MaximumLength(100)
                                         .Matches(@"^[A-Z]+$").WithMessage("ShortCode must be uppercase letters only, no spaces.");



        }

    }
}
