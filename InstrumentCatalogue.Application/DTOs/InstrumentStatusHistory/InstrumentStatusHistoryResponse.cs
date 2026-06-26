using InstrumentCatalogue.Core.Enums;

namespace InstrumentCatalogue.Application.DTOs.InstrumentStatusHistory;

public record InstrumentStatusHistoryResponse
{
    public Guid InstrumentStatusHistoryId { get; set; }

    public Guid InstrumentId { get; set; }

    public DateOnly ValidFrom { get; set; }

    public DateOnly ValidTo { get; set; }

    public DateOnly EffectiveDate { get; set; }

    public string? Notes { get; set; }

    public InstrumentStatus InstrumentStatus { get; set; }

}
