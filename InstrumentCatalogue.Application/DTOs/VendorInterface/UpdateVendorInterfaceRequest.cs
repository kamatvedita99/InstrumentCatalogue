namespace InstrumentCatalogue.Application.DTOs.VendorInterface;

public class UpdateVendorInterfaceRequest
{
    public string? Name { get; set; }

    public string? Protocol { get; set; }

    public string? Description { get; set; }

    public bool? IsActive { get; set; }
}
