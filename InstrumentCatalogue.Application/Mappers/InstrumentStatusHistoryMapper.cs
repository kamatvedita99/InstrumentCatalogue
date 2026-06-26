using InstrumentCatalogue.Application.DTOs.InstrumentStatusHistory;
using InstrumentCatalogue.Core.Models;

namespace InstrumentCatalogue.Application.Mappers;

public static class InstrumentStatusHistoryMapper
{
    public static InstrumentStatusHistoryResponse ToResponse(InstrumentStatusHistory instrumentStatusHistory)
    {
        ArgumentNullException.ThrowIfNull(instrumentStatusHistory);

        return new InstrumentStatusHistoryResponse
        {
            InstrumentStatusHistoryId = instrumentStatusHistory.InstrumentStatusHistoryId,
            InstrumentId = instrumentStatusHistory.InstrumentId,
            InstrumentStatus = instrumentStatusHistory.InstrumentStatus,
            EffectiveDate = instrumentStatusHistory.EffectiveDate,
            ValidFrom = instrumentStatusHistory.ValidFrom,
            ValidTo = instrumentStatusHistory.ValidTo,
            Notes = instrumentStatusHistory.Notes,
        };
    }
}
