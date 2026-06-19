namespace InstrumentCatalogue.Core.Interfaces.Shared
{
    public interface ITimeStampAudit
    {
        DateTime CreatedAtUtc { get; set; }

        DateTime LastUpdatedAtUtc { get; set; }
    }
}
