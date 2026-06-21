using FluentValidation;
using InstrumentCatalogue.Application.DTOs.Instrument;

namespace InstrumentCatalogue.Application.Validators.Instrument;

public class CreateEtfRefRequestValidator : AbstractValidator<CreateEtfRefRequest>
{
    public CreateEtfRefRequestValidator()
    {
        
        RuleFor(e => e.ReplicationType)
            .IsInEnum()
            .WithMessage("ReplicationType is not a recognised value.");

        RuleFor(e => e.ExpenseRatio)
            .GreaterThanOrEqualTo(0)
            .WithMessage("ExpenseRatio cannot be negative.");

        RuleFor(e => e.FundManager)
            .MaximumLength(300);

        RuleFor(e => e.UnderlyingIndex)
            .MaximumLength(200);

        RuleFor(e => e.DistributionFrequency)
            .IsInEnum()
            .WithMessage("DistributionFrequency is not a recognised value.");
    }
}