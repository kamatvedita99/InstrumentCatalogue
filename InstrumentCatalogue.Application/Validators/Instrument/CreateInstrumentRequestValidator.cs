using FluentValidation;
using InstrumentCatalogue.Application.DTOs.Instrument;
using InstrumentCatalogue.Core.Enums;

namespace InstrumentCatalogue.Application.Validators.Instrument;

public class CreateInstrumentRequestValidator: AbstractValidator<CreateInstrumentRequest>
{
    public CreateInstrumentRequestValidator()
    {
        RuleFor(cir => cir.BondRef).NotNull().SetValidator(new CreateBondRefRequestValidator()).When(cir => cir.Type == InstrumentType.Bond);

        RuleFor(cir => cir.EquityRef).NotNull().SetValidator(new CreateEquityRefRequestValidator()).When(cir => cir.Type == InstrumentType.Equity);

        RuleFor(cir => cir.EtfRef).NotNull().SetValidator(new CreateEtfRefRequestValidator()).When(cir => cir.Type == InstrumentType.ETF);

        RuleFor(cir => cir.Name).NotEmpty();

        RuleFor(cir => cir.Country).NotEmpty()
                                    .MinimumLength(3)
                                    .MaximumLength(3)
                                    .Matches(@"^[A-Z]+$").WithMessage("Country must be in ISO Code format, uppercase alphabets only");

        RuleFor(cir => cir.Currency).NotEmpty()
                                    .MinimumLength(3)
                                    .MaximumLength(3)
                                    .Matches(@"^[A-Z]+$").WithMessage("Currency must be in ISO Code format, uppercase alphabets only");

        RuleFor(cir => cir.Exchange).NotEmpty()
                                    .Matches(@"^[^0-9]*$").WithMessage("Exchange cannot contain numbers");

        RuleFor(cir => cir.Symbols).NotEmpty().WithMessage("At least one symbol is required.");

        RuleForEach(cir => cir.Symbols)
                .SetValidator(new CreateInstrumentSymbolRequestValidator());


    }

}
