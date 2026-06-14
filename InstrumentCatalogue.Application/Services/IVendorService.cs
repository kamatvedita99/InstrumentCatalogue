using InstrumentCatalogue.Application.DTOs;

namespace InstrumentCatalogue.Application.Services;

public interface IVendorService
{
    Task<VendorResponse> CreateVendorAsync(CreateVendorRequest request, CancellationToken cancellationToken);
}
