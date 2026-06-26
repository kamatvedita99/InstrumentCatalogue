namespace InstrumentCatalogue.Application.DTOs.Instrument;

public record EquityRefResponse
{
    public Guid InstrumentId { get; set; }

    public string? Sector { get; set; }

    public string? Industry { get; set; }

    public long? SharesOutstanding { get; set; }

    public int LotSize { get; set; }

    public decimal? ParValue { get; set; }
}
