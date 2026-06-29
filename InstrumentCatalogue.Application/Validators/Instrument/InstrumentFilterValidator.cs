using FluentValidation;
using InstrumentCatalogue.Core.Common;
using InstrumentCatalogue.Core.Enums;
using InstrumentCatalogue.Core.Filters;

namespace InstrumentCatalogue.Application.Validators.Instrument;

public class InstrumentFilterValidator : AbstractValidator<InstrumentFilter>
{
    public InstrumentFilterValidator()
    {

        RuleFor(ifv => ifv.Type)
            .NotNull()
            .When(f => f.BondFilter != null || f.EquityFilter != null || f.EtfFilter != null)
            .WithMessage("Type must be specified when a type-specific filter is provided.");

        RuleFor(f => f.BondFilter)
            .Null()
            .When(f => f.Type is not null && f.Type != InstrumentType.Bond)
            .WithMessage("BondFilter can only be used when Type is Bond.");

        RuleFor(f => f.EquityFilter)
            .Null()
            .When(f => f.Type is not null && f.Type != InstrumentType.Equity)
            .WithMessage("EquityFilter can only be used when Type is Equity.");

        RuleFor(f => f.EtfFilter)
            .Null()
            .When(f => f.Type is not null && f.Type != InstrumentType.ETF)
            .WithMessage("ETF Filter can only be used when Type is ETF.");



    }
}
