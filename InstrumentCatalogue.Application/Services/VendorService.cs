using InstrumentCatalogue.Application.DTOs;
using InstrumentCatalogue.Application.Mappers;
using InstrumentCatalogue.Core.Interfaces;

namespace InstrumentCatalogue.Application.Services;

public class VendorService : IVendorService
{
    private readonly IVendorRepository _vendorRepository;

    public VendorService(IVendorRepository vendorRepository)
    {
        _vendorRepository = vendorRepository ?? throw new ArgumentNullException(nameof(vendorRepository));
    }
    public async Task<VendorResponse> CreateVendorAsync(CreateVendorRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var vendorModel = VendorMapper.ToDomain(request);
        await _vendorRepository.CreateVendorAsync(vendorModel, cancellationToken);
        return VendorMapper.ToResponse(vendorModel);

    }
}
