using InstrumentCatalogue.Application.DTOs.Instrument;
using InstrumentCatalogue.Application.Extensions;
using InstrumentCatalogue.Core.Models;

namespace InstrumentCatalogue.Application.Mappers;

public static class EquityRefDataMapper
{
    public static EquityRefData ToDomain(CreateInstrumentRequest request)
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

    public static EquityRefResponse ToResponse(EquityRefData equityRefData)
    {
        ArgumentNullException.ThrowIfNull(equityRefData);

        return new EquityRefResponse
        {
            InstrumentId = equityRefData.InstrumentId,
            Sector = equityRefData?.Sector,
            Industry = equityRefData?.Industry,
            SharesOutstanding = equityRefData?.SharesOutstanding,
            LotSize = equityRefData.LotSize,
            ParValue = equityRefData.ParValue


        };
    }
}