namespace InstrumentCatalogue.Core.Models;

public class Vendor
{   
    public int VendorId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string ShortCode { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime LastUpdatedAtUtc { get;  set; }

    //navigation properties
    public ICollection<VendorInterface> Interfaces { get; set; } = new List<VendorInterface>();
}
