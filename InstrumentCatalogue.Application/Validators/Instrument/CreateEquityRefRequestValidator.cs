using FluentValidation;
using InstrumentCatalogue.Application.DTOs.Instrument;

namespace InstrumentCatalogue.Application.Validators.Instrument;

public class CreateEquityRefRequestValidator : AbstractValidator<CreateEquityRefRequest>
{
    public CreateEquityRefRequestValidator()
    {
        
        RuleFor(e => e.LotSize)
            .GreaterThan(0)
            .WithMessage("LotSize must be greater than zero.");

        RuleFor(e => e.SharesOutstanding)
            .GreaterThanOrEqualTo(0)
            .WithMessage("SharesOutstanding cannot be negative.");

        RuleFor(e => e.ParValue)
            .GreaterThanOrEqualTo(0)
            .WithMessage("ParValue cannot be negative.");

        RuleFor(e => e.Sector)
            .MaximumLength(100);

        RuleFor(e => e.Industry)
            .MaximumLength(200);
    }
}