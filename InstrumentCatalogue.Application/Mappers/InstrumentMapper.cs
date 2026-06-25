using InstrumentCatalogue.Application.DTOs.Instrument;
using InstrumentCatalogue.Application.Exceptions;
using InstrumentCatalogue.Application.Extensions;
using InstrumentCatalogue.Core.Enums;
using InstrumentCatalogue.Core.Models;

namespace InstrumentCatalogue.Application.Mappers;

public class InstrumentMapper
{
    private readonly Dictionary<InstrumentType, IRefDataMapper> _instrumentRefDataMapping;
    public InstrumentMapper(Dictionary<InstrumentType, IRefDataMapper> instrumentRefDataMapping)
    {
        _instrumentRefDataMapping = instrumentRefDataMapping;
    }

    public Instrument ToDomain(CreateInstrumentRequest request, Dictionary<string, int> symbologyMapper)
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

        var refDataMapper = _instrumentRefDataMapping[request.Type];
        var instrumentRefData = refDataMapper.Map(request);

        switch (request.Type)
        {
            case InstrumentType.Bond:
                instrument.BondRefData = (BondRefData)instrumentRefData;
                break;
            case InstrumentType.Equity:
                instrument.EquityRefData = (EquityRefData)instrumentRefData;
                break;
            case InstrumentType.ETF:
                instrument.EtfRefData = (EtfRefData)instrumentRefData;
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

                }.StampCreated();

            }).ToList();
    

        instrument.StampCreated();

        return instrument;
    }
}
