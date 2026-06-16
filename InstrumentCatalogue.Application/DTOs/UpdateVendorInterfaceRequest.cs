namespace InstrumentCatalogue.Application.DTOs;

public class UpdateVendorInterfaceRequest
{
    public string? Name { get; set; }

    public string? Protocol { get; set; }

    public string? Description { get; set; }

    public bool? IsActive { get; set; }
}
