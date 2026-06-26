using InstrumentCatalogue.Application.DTOs.Symbology;
using InstrumentCatalogue.Application.DTOs.VendorInterfaceSymbol;
using InstrumentCatalogue.Core.Models;

namespace InstrumentCatalogue.Application.DTOs.SymbolXRef;

public record SymbolXRefResponse
{
    public Guid SymbolXRefId { get; set; }
    public int SymbologyId { get; set; }

    public string Symbol { get; set; } = string.Empty;

    public Guid InstrumentId { get; set; }

    public bool? IsPrimary { get; set; }

    public DateOnly ValidFrom { get; set; }

    public DateOnly ValidTo { get; set; }

    public SymbologyResponse Symbology { get; set; } = null!;

    public ICollection<VendorInterfaceSymbolResponse> VendorInterfaceSymbols { get; set; } = new List<VendorInterfaceSymbolResponse>();
}
