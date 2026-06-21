using FluentValidation;
using InstrumentCatalogue.Application.DTOs.VendorInterface;

namespace InstrumentCatalogue.Application.Validators.VendorInterface;

public class CreateVendorInterfaceRequestValidator : AbstractValidator<CreateVendorInterfaceRequest>
{
    public CreateVendorInterfaceRequestValidator()
    {
        RuleFor(cvir => cvir.Name).NotEmpty()
                                   .MaximumLength(100)
                                   .Matches(@"^[^0-9]*$").WithMessage("Name cannot contain numbers");

        RuleFor(cvir => cvir.Protocol)
                                     .MaximumLength(100);


        RuleFor(cvir => cvir.Description)
                                   .MaximumLength(250)
                                   .Matches(@"^[^0-9]*$").WithMessage("Description cannot contain numbers");

    }
}
