using InstrumentCatalogue.Application.DTOs;
using InstrumentCatalogue.Application.Mappers;
using InstrumentCatalogue.Core.Interfaces;
using InstrumentCatalogue.Core.Models;

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

    public async Task<VendorResponse?> GetVendorByIdAsync(int vendorId, CancellationToken cancellationToken)
    {
       var vendor = await _vendorRepository.GetVendorByIdAsync(vendorId, cancellationToken);
       if(vendor == null) 
            return null;
        
        return VendorMapper.ToResponse(vendor);
    }

    public async Task<ICollection<VendorResponse>> GetVendorsAsync(CancellationToken cancellationToken)
    {
        var vendorList = await _vendorRepository.GetVendorsAsync(cancellationToken);
        return vendorList.Select(v => VendorMapper.ToResponse(v)).ToList();
    }

    public async Task<VendorResponse?> UpdateVendorAsync(int vendorId, UpdateVendorRequest vendorUpdateRequest, CancellationToken cancellationToken)
    {
        var vendor = await _vendorRepository.GetVendorByIdAsync(vendorId, cancellationToken);

        if(vendor == null)
            return null;

        if (!string.IsNullOrWhiteSpace(vendorUpdateRequest.Name))
            vendor.Name = vendorUpdateRequest.Name;

        if (vendorUpdateRequest.IsActive.HasValue)
            vendor.IsActive = vendorUpdateRequest.IsActive.Value;

        vendor.LastUpdatedAtUtc = DateTime.UtcNow;

        await _vendorRepository.UpdateVendorAsync(vendor, cancellationToken);

        return VendorMapper.ToResponse(vendor);


    }
}
