using FluentValidation;
using InstrumentCatalogue.Application.DTOs;

namespace InstrumentCatalogue.Application.Validators;

public class UpdateVendorRequestValidator : AbstractValidator<UpdateVendorRequest>
{
    public UpdateVendorRequestValidator()
    {
        RuleFor(cvr => cvr.Name)
                                   .MaximumLength(100)
                                   .Matches(@"^[^0-9]*$").WithMessage("Name cannot contain numbers");


    }
}
