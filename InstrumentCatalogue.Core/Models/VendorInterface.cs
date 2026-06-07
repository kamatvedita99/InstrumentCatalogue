namespace InstrumentCatalogue.Core.Models;

public class VendorInterface
{
    public int VendorInterfaceId { get; set; }

    public int VendorId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Protocol { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime LastUpdatedAtUtc { get; set; }

    //navigation properties
    public Vendor? Vendor { get; set; } = null!;

}
