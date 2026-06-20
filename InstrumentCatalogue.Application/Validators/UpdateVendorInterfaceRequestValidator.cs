using FluentValidation;
using InstrumentCatalogue.Application.DTOs;

namespace InstrumentCatalogue.Application.Validators;

public class UpdateVendorInterfaceRequestValidator : AbstractValidator<UpdateVendorInterfaceRequest>
{
    public UpdateVendorInterfaceRequestValidator()
    {
        RuleFor(uvir => uvir.Name).MaximumLength(200)
                                   .Matches(@"^[^0-9]*$").WithMessage("Description cannot contain numbers");

        RuleFor(uvir => uvir.Description).MaximumLength(250)
                                         .Matches(@"^[^0-9]*$").WithMessage("Description cannot contain numbers");


        RuleFor(uvir => uvir.Protocol).MaximumLength(100);
    }
}
