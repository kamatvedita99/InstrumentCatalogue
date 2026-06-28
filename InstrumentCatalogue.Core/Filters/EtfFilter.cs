using InstrumentCatalogue.Core.Enums;

namespace InstrumentCatalogue.Core.Filters;

public class EtfFilter
{
    public string? UnderlyingIndex { get; set; }
    public EtfReplicationType? ReplicationType { get; set; }
    public EtfDistributionFrequency? DistributionFrequency { get; set; }
    public DateOnly? InceptionDate { get; set; }
}
