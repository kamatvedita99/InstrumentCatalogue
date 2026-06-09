namespace InstrumentCatalogue.Core.Models;

public class SymbolXRef
{
    public Guid SymbolXRefId { get; set; }

    public int SymbologyId { get; set; }

    public string Symbol { get; set; } = string.Empty;

    public Guid InstrumentId { get; set; }

    public bool? IsPrimary { get; set; }

    public DateOnly ValidFrom { get; set; }

    public DateOnly ValidTo { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime LastUpdatedAtUtc { get; set; }

    //navigation properties
    public Instrument Instrument { get; set; } = null!;

    public Symbology Symbology { get; set; } = null!;

}
