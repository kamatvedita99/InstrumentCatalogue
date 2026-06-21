namespace InstrumentCatalogue.Application.DTOs.Instrument;

public class CreateEquityRefRequest
{
    public string? Sector { get; set; }
    public string? Industry { get; set; }
    public long? SharesOutstanding { get; set; }
    public int LotSize { get; set; } = 1;
    public decimal? ParValue { get; set; }
}
