using FluentValidation;
using InstrumentCatalogue.Application.DTOs.Instrument;

namespace InstrumentCatalogue.Application.Validators.Instrument
{
    public class UpdateInstrumentRequestValidator :AbstractValidator<UpdateInstrumentRequest>
    {
        public UpdateInstrumentRequestValidator()
        {
            RuleFor(cir => cir.Country)
                                    .Length(2)
                                    .Matches(@"^[A-Z]+$").WithMessage("Country must be a valid ISO 3166 alpha-2 code (e.g. IN, US).")
                                    .When(cir => cir.Country != null);


            RuleFor(cir => cir.Currency)
                                        .MinimumLength(3)
                                        .MaximumLength(3)
                                        .Matches(@"^[A-Z]+$").WithMessage("Currency must be in ISO Code format, uppercase alphabets only")
                                        .When(cir => cir.Currency != null);

            RuleFor(cir => cir.Exchange)
                                        .Matches(@"^[^0-9]*$").WithMessage("Exchange cannot contain numbers")
                                            .When(cir => cir.Exchange != null);

            RuleFor(cir => cir)
                                        .Must(x => new[] { x.BondRef != null, x.EquityRef != null, x.EtfRef != null }
                                        .Count(b => b) <= 1)
                                        .WithMessage("Only one ref data type can be provided.");

            RuleFor(x => x.Name)
                                .NotEmpty()
                                .When(x => x.Name != null);

            RuleFor(x => x.ListedDate)
                                .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
                                .WithMessage("Listed date cannot be in the future.")
                                .When(x => x.ListedDate != null);
        }
    }
}
