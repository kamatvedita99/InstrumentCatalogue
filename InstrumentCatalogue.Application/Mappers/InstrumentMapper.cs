using InstrumentCatalogue.Application.DTOs.Instrument;
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

    public Instrument ToDomain(CreateInstrumentRequest request)
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

        switch(request.Type)
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

        return instrument;
    }
}
