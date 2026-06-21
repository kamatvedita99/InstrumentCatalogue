using FluentValidation;
using InstrumentCatalogue.Application.DTOs.Symbology;

namespace InstrumentCatalogue.Application.Validators.Symbology;

public class UpdateSymbologyRequestValidator : AbstractValidator<UpdateSymbologyRequest>
{
    public UpdateSymbologyRequestValidator()
    {
        RuleFor(usr => usr.Description)
                                   .MaximumLength(250)
                                   .Matches(@"^[^0-9]*$").WithMessage("Description cannot contain numbers");

    }
}
