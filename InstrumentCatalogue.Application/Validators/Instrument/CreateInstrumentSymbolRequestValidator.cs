using FluentValidation;
using InstrumentCatalogue.Application.DTOs.Instrument;

namespace InstrumentCatalogue.Application.Validators.Instrument;

public class CreateInstrumentSymbolRequestValidator : AbstractValidator<CreateInstrumentSymbolRequest>
{
    public CreateInstrumentSymbolRequestValidator()
    {
        RuleFor(cir => cir.SymbologyTypeCode).NotEmpty()
                                    .MaximumLength(100)
                                    .Matches(@"^[A-Z]+$").WithMessage("TypeCode must be uppercase only, no spaces");

        RuleFor(cir => cir.SymbolName).NotEmpty();


                                    
    }
}
