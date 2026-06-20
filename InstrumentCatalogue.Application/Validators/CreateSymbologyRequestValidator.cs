using FluentValidation;
using InstrumentCatalogue.Application.DTOs;

namespace InstrumentCatalogue.Application.Validators
{
    public class CreateSymbologyRequestValidator : AbstractValidator<CreateSymbologyRequest>
    {

        public CreateSymbologyRequestValidator()
        {
            RuleFor(csr => csr.TypeCode).NotEmpty()
                                        .MaximumLength(100)
                                        .Matches(@"^[A-Z]+$").WithMessage("TypeCode must be uppercase only, no spaces");


            RuleFor(csr => csr.Description).NotEmpty()
                                   .MaximumLength(250)
                                   .Matches(@"^[^0-9]*$").WithMessage("Description cannot contain numbers");

        }
    }
}
