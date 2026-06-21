namespace InstrumentCatalogue.Application.DTOs.Instrument;

public class CreateEtfRefRequest
{
    public string? FundManager { get; set; }
    public string? UnderlyingIndex { get; set; }
    public EtfReplicationType ReplicationType { get; set; }
    public EtfDistributionFrequency? DistributionFrequency { get; set; }
    public DateOnly? InceptionDate { get; set; }
    public decimal? ExpenseRatio { get; set; }
}
