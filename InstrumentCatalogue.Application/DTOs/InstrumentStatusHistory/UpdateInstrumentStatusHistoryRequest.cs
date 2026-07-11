using InstrumentCatalogue.Core.Enums;

namespace InstrumentCatalogue.Application.DTOs.InstrumentStatusHistory;

public class UpdateInstrumentStatusHistoryRequest
{
    public DateOnly EffectiveDate { get; set; }

    public string? Notes { get; set; }

    public InstrumentStatus InstrumentStatus { get; set; }
}
