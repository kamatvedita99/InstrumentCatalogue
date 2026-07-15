using FluentValidation;
using InstrumentCatalogue.Application.DTOs.SymbolXRef;

namespace InstrumentCatalogue.Application.Validators.SymbolXRef;

public class UpdateSymbolXRefRequestValidator : AbstractValidator<UpdateSymbolXRefRequest>
{
    public UpdateSymbolXRefRequestValidator()
    {
        RuleFor(usxr => usxr.IsPrimary).NotEmpty().WithMessage("IsPrimary is required field for updation");
    }
}
