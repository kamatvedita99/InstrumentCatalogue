namespace InstrumentCatalogue.Application.DTOs.VendorInterface;

public record VendorInterfaceResponse(int VendorInterfaceId, int VendorId, string Name, string? Description, string? Protocol, bool IsActive);
