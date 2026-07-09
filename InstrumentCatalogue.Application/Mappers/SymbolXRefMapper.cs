using InstrumentCatalogue.Application.DTOs.Instrument;
using InstrumentCatalogue.Application.DTOs.SymbolXRef;
using InstrumentCatalogue.Application.Extensions;
using InstrumentCatalogue.Core.Models;

namespace InstrumentCatalogue.Application.Mappers;

public static class SymbolXRefMapper
{
    public static SymbolXRefResponse ToResponse(SymbolXRef symbolXRef)
    {
        ArgumentNullException.ThrowIfNull(symbolXRef);

        var symbolXRefResponse =  new SymbolXRefResponse
        {
            SymbolXRefId = symbolXRef.SymbolXRefId,
            Symbol = symbolXRef.Symbol,
            SymbologyId = symbolXRef.SymbologyId,
            IsPrimary = symbolXRef.IsPrimary,
            InstrumentId = symbolXRef.InstrumentId,
            ValidFrom = symbolXRef.ValidFrom,
            ValidTo = symbolXRef.ValidTo,
            VendorInterfaceSymbols = symbolXRef.VendorInterfaceSymbols.Select(VendorInterfaceSymbolMapper.ToResponse).ToList(),
            
        };

        if (symbolXRef.Symbology != null)
            symbolXRefResponse.Symbology = SymbologyMapper.ToResponse(symbolXRef.Symbology);

        return symbolXRefResponse;
    }

    public static SymbolXRef ToDomain(Guid instrumentId, int symbologyId, CreateInstrumentSymbolRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return new SymbolXRef
        {
            InstrumentId = instrumentId,
            SymbologyId = symbologyId,
            IsPrimary = request.IsPrimary,
            Symbol = request.SymbolName,
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow)

        }.StampCreated();
    }
}
