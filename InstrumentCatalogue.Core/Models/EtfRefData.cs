using InstrumentCatalogue.Core.Enums;

namespace InstrumentCatalogue.Core.Models;

public class EtfRefData
{
    public Guid InstrumentId { get; set; }
    public string? FundManager { get; set; }
    public string? UnderlyingIndex { get; set; }
    public EtfReplicationType ReplicationType { get; set; }
    public EtfDistributionFrequency? DistributionFrequency { get; set; }
    public DateOnly? InceptionDate { get; set; }
    public decimal? ExpenseRatio { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime LastUpdatedAtUtc { get; set; }
}
