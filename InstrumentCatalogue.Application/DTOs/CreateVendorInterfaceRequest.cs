namespace InstrumentCatalogue.Application.DTOs;

public class CreateVendorInterfaceRequest
{
    public string Name { get; set; } = string.Empty;

    public string? Protocol { get; set; }

    public string? Description { get; set; }

}
