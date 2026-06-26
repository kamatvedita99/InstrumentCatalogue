namespace InstrumentCatalogue.Application.DTOs.VendorInterfaceSymbol;

public record VendorInterfaceSymbolResponse
{
    public Guid VendorInterfaceSymbolXRefId { get; set; }

    public int VendorInterfaceId { get; set; }

    public Guid SymbolXRefId { get; set; }

    public DateTime ReceivedAtUtc { get; set; }

    public bool IsActive { get; set; }
}
