using InstrumentCatalogue.Application.DTOs.Instrument;
using InstrumentCatalogue.Application.Extensions;
using InstrumentCatalogue.Core.Interfaces.Shared;
using InstrumentCatalogue.Core.Models;

namespace InstrumentCatalogue.Application.Mappers;

public class EquityRefDataMapper : IRefDataMapper
{
    public IInstrumentRefData Map(CreateInstrumentRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var equityRefRequest = request.EquityRef;
        ArgumentNullException.ThrowIfNull(equityRefRequest);

        var equityRef = new EquityRefData
        {
            Sector = equityRefRequest.Sector,
            Industry = equityRefRequest.Industry,
            SharesOutstanding = equityRefRequest.SharesOutstanding,
            LotSize = equityRefRequest.LotSize,
            ParValue = equityRefRequest.ParValue,
        };

        equityRef.StampCreated();
        return equityRef;
    }
}