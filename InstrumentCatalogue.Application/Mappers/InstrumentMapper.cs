using InstrumentCatalogue.Application.DTOs.Instrument;
using InstrumentCatalogue.Application.Exceptions;
using InstrumentCatalogue.Application.Extensions;
using InstrumentCatalogue.Core.Enums;
using InstrumentCatalogue.Core.Models;

namespace InstrumentCatalogue.Application.Mappers;

public static class InstrumentMapper
{
    
    public static Instrument ToDomain(CreateInstrumentRequest request, Dictionary<string, int> symbologyMapper)
    {
        ArgumentNullException.ThrowIfNull(request);

        var instrument = new Instrument()
        {
            Country = request.Country,
            Currency = request.Currency,
            Exchange = request.Exchange,
            Name = request.Name,
            Type = request.Type,
            ListedDate = request.ListedDate,

        };

        switch (request.Type)
        {
            case InstrumentType.Bond:
                instrument.BondRefData = BondRefDataMapper.ToDomain(request);
                break;
            case InstrumentType.Equity:
                instrument.EquityRefData = EquityRefDataMapper.ToDomain(request);
                break;
            case InstrumentType.ETF:
                instrument.EtfRefData = EtfRefDataMapper.ToDomain(request);
                break;

        }

        instrument.Symbols = request.Symbols.Select(
            (symbol) =>

            {
                if (!symbologyMapper.TryGetValue(symbol.SymbologyTypeCode, out var symbologyId))
                    throw new NotFoundException<string>(nameof(Symbology), symbol.SymbologyTypeCode);

                return new SymbolXRef
                {
                    SymbologyId = symbologyId,
                    Symbol = symbol.SymbolName,
                    IsPrimary = symbol.IsPrimary,
                    ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow),

                }.StampCreated();

            }).ToList();
    

        instrument.StampCreated();

        return instrument;
    }


    public static InstrumentResponse ToResponse(Instrument instrument)
    {
        ArgumentNullException.ThrowIfNull(instrument);

        var instrumentResponse =  new InstrumentResponse
        {
            InstrumentId = instrument.InstrumentId,
            Country = instrument.Country,
            Currency = instrument.Currency,
            Exchange = instrument.Exchange,
            Name = instrument.Name,
            Type = instrument.Type,
            ListedDate = instrument.ListedDate,
            InstrumentStatusHistory = instrument.StatusHistory.Select(InstrumentStatusHistoryMapper.ToResponse).ToList(),
            Symbols = instrument.Symbols.Select(SymbolXRefMapper.ToResponse).ToList(),

        };

        switch (instrument.Type)
        {
            case InstrumentType.Bond:
                instrumentResponse.BondRef = BondRefDataMapper.ToResponse(instrument.BondRefData!);
                break;
            case InstrumentType.Equity:
                instrumentResponse.EquityRef = EquityRefDataMapper.ToResponse(instrument.EquityRefData!);
                break;
            case InstrumentType.ETF:
                instrumentResponse.EtfRef = EtfRefDataMapper.ToResponse(instrument.EtfRefData!);
                break;

        }

        return instrumentResponse;
    }
}
