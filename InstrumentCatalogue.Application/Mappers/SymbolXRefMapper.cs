using InstrumentCatalogue.Application.DTOs.SymbolXRef;
using InstrumentCatalogue.Core.Models;

namespace InstrumentCatalogue.Application.Mappers;

public static class SymbolXRefMapper
{
    public static SymbolXRefResponse ToResponse(SymbolXRef symbolXRef)
    {
        ArgumentNullException.ThrowIfNull(symbolXRef);

        return new SymbolXRefResponse
        {
            SymbolXRefId = symbolXRef.SymbolXRefId,
            Symbol = symbolXRef.Symbol,
            SymbologyId = symbolXRef.SymbologyId,
            IsPrimary = symbolXRef.IsPrimary,
            Symbology = SymbologyMapper.ToResponse(symbolXRef.Symbology),
            InstrumentId = symbolXRef.InstrumentId,
            ValidFrom = symbolXRef.ValidFrom,
            ValidTo = symbolXRef.ValidTo,
            VendorInterfaceSymbols = symbolXRef.VendorInterfaceSymbols.Select(VendorInterfaceSymbolMapper.ToResponse).ToList()
        };
    }
}
