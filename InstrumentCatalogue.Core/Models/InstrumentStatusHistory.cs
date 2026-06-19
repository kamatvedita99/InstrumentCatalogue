using InstrumentCatalogue.Core.Enums;
using InstrumentCatalogue.Core.Interfaces.Shared;

namespace InstrumentCatalogue.Core.Models;

public class InstrumentStatusHistory : ITimeStampAudit
{
    public Guid InstrumentStatusHistoryId { get; set; }

    public Guid InstrumentId { get; set;}

    public DateOnly ValidFrom { get; set; }

    public DateOnly ValidTo { get; set; }

    public DateOnly EffectiveDate { get; set; }

    public string? Notes { get; set; }

    public InstrumentStatus InstrumentStatus { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime LastUpdatedAtUtc { get; set; }

    //navigation properties
    //not needed to reference from History to Symbol

}
