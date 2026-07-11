using FluentValidation;
using InstrumentCatalogue.Application.DTOs.InstrumentStatusHistory;

namespace InstrumentCatalogue.Application.Validators.InstrumentStatusHistory;

public class UpdateInstrumentStatusHistoryRequestValidator : AbstractValidator<UpdateInstrumentStatusHistoryRequest>
{
    public UpdateInstrumentStatusHistoryRequestValidator()
    {
        RuleFor(uishr => uishr.EffectiveDate).LessThanOrEqualTo(DateOnly.MaxValue).WithMessage("Effective date should be within range");
        RuleFor(uishr => uishr.Notes).MaximumLength(500);

        RuleFor(uishr => uishr.InstrumentStatus)
            .IsInEnum()
            .WithMessage("InstrumentStatus value provided is not recognised.");
    }
}
