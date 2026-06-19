using InstrumentCatalogue.Core.Interfaces.Shared;

namespace InstrumentCatalogue.Core.Models;

public class VendorInterfaceSymbolXRef : ITimeStampAudit
{
    public Guid VendorInterfaceSymbolXRefId { get; set; }

    public int VendorInterfaceId { get; set; }

    public Guid SymbolXRefId { get; set; }

    public DateTime ReceivedAtUtc { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime LastUpdatedAtUtc { get; set; }



}
