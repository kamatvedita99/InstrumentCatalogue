namespace InstrumentCatalogue.Application.DTOs;

public record VendorInterfaceResponse(int VendorInterfaceId, int VendorId, string Name, string? Description, string? Protocol, bool IsActive );
