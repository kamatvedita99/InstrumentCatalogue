using FluentValidation;
using InstrumentCatalogue.Application.DTOs.Instrument;

namespace InstrumentCatalogue.Application.Validators.Instrument;

public class CreateBondRefRequestValidator : AbstractValidator<CreateBondRefRequest>
{
    public CreateBondRefRequestValidator()
    {
        RuleFor(b => b.FaceValue)
            .GreaterThan(0)
            .WithMessage("FaceValue is required and must be greater than zero.");

        RuleFor(b => b.CouponRate)
            .GreaterThanOrEqualTo(0)
            .WithMessage("CouponRate cannot be negative.");

        RuleFor(b => b.Duration)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Duration cannot be negative.");

        // .When() needed here — comparing IssueDate.Value would throw if IssueDate were null
        RuleFor(b => b.MaturityDate)
            .GreaterThan(b => b.IssueDate!.Value)
            .When(b => b.IssueDate.HasValue && b.MaturityDate.HasValue)
            .WithMessage("MaturityDate must be after IssueDate.");

        RuleFor(b => b.Issuer)
            .MaximumLength(150);

        RuleFor(b => b.CreditRating)
            .MaximumLength(25);
    }
}