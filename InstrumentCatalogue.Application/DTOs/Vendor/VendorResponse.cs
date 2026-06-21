namespace InstrumentCatalogue.Application.DTOs.Vendor;

public record VendorResponse(int VendorId, string Name, string ShortCode, bool IsActive);
