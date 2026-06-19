using InstrumentCatalogue.Core.Interfaces.Shared;

namespace InstrumentCatalogue.Core.Models;

public class Symbology : ITimeStampAudit
{
    public int SymbologyId { get; set; }
    public string TypeCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime LastUpdatedAtUtc { get;  set; }
}
