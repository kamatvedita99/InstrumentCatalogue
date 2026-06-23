using InstrumentCatalogue.Core.Interfaces.Shared;

namespace InstrumentCatalogue.Core.Models;

public class EquityRefData : ITimeStampAudit, IInstrumentRefData
{
    public Guid InstrumentId { get; set; }
    public string? Sector { get; set; }
    public string? Industry { get; set; }
    public long? SharesOutstanding { get; set; }
    public int LotSize { get; set; } = 1;
    public decimal? ParValue { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime LastUpdatedAtUtc { get; set; }
}
