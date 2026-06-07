namespace InstrumentCatalogue.Core.Models;

public class Symbology
{
    public int SymbologyId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; }

    public DateTime LastUpdatedAtUtc { get;  set; }
}
