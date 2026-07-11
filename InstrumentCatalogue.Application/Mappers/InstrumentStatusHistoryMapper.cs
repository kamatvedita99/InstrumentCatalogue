using InstrumentCatalogue.Application.DTOs.InstrumentStatusHistory;
using InstrumentCatalogue.Application.Extensions;
using InstrumentCatalogue.Core.Constants;
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

    public static InstrumentStatusHistory ToDomain(Guid instrumentId, UpdateInstrumentStatusHistoryRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return new InstrumentStatusHistory
        {
            
            InstrumentId = instrumentId,
            InstrumentStatus = request.InstrumentStatus,
            EffectiveDate = request.EffectiveDate,
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow),
            ValidTo = DateOnly.Parse(TemporalDefaults.CurrentSentinelSql),
            Notes = request.Notes,
        }.StampCreated();
    }
}
