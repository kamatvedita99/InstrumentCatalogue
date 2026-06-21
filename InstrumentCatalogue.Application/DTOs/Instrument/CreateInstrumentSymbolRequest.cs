namespace InstrumentCatalogue.Application.DTOs.Instrument;

public class CreateInstrumentSymbolRequest
{
    public string SymbologyTypeCode { get; set; } = string.Empty;

    public string SymbolName { get; set; } = string.Empty;

    public bool? IsPrimary { get; set; }
}
