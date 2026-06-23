using InstrumentCatalogue.Core.Enums;
using InstrumentCatalogue.Core.Interfaces.Shared;

namespace InstrumentCatalogue.Core.Models;

public class EtfRefData : ITimeStampAudit, IInstrumentRefData
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
