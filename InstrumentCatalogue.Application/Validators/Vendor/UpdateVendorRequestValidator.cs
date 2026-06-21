using FluentValidation;
using InstrumentCatalogue.Application.DTOs.Vendor;

namespace InstrumentCatalogue.Application.Validators.Vendor;

public class UpdateVendorRequestValidator : AbstractValidator<UpdateVendorRequest>
{
    public UpdateVendorRequestValidator()
    {
        RuleFor(cvr => cvr.Name)
                                   .MaximumLength(100)
                                   .Matches(@"^[^0-9]*$").WithMessage("Name cannot contain numbers");


    }
}
