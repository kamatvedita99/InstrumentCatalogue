using InstrumentCatalogue.Application.DTOs;
using InstrumentCatalogue.Application.Exceptions;
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

    public async Task<VendorInterfaceResponse> CreateVendorInterfaceAsync(int vendorId, CreateVendorInterfaceRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var vendorInterface = VendorInterfaceMapper.ToDomain(vendorId, request);
        await _vendorRepository.CreateVendorInterfaceAsync(vendorInterface, cancellationToken);

        return VendorInterfaceMapper.ToResponse(vendorInterface);
    }

    public async Task<VendorResponse?> GetVendorByIdAsync(int vendorId, CancellationToken cancellationToken = default)
    {
       var vendor = await _vendorRepository.GetVendorByIdAsync(vendorId, cancellationToken);
       
        if(vendor == null) 
            throw new NotFoundException<int>(nameof(vendor), vendorId);
        
        return VendorMapper.ToResponse(vendor);
    }

    public async Task<VendorInterfaceResponse?> GetVendorInterfaceByIdAsync(int vendorInterfaceId, CancellationToken cancellationToken = default)
    {
        var vendorInterface = await _vendorRepository.GetVendorInterfaceByIdAsync(vendorInterfaceId, cancellationToken);
        
        if(vendorInterface == null)
            throw new NotFoundException<int>(nameof(vendorInterface), vendorInterfaceId);

        return VendorInterfaceMapper.ToResponse(vendorInterface);
    }

    public async Task<ICollection<VendorInterfaceResponse>> GetVendorInterfacesAsync(int vendorId, CancellationToken cancellationToken = default)
    {
        var vendorInterfacesList = await _vendorRepository.GetVendorInterfacesAsync(vendorId, cancellationToken);
        return vendorInterfacesList.Select(VendorInterfaceMapper.ToResponse).ToList();
    }

    public async Task<ICollection<VendorResponse>> GetVendorsAsync(CancellationToken cancellationToken = default)
    {
        var vendorList = await _vendorRepository.GetVendorsAsync(cancellationToken);
        return vendorList.Select(VendorMapper.ToResponse).ToList();
    }

    public async Task<VendorResponse?> UpdateVendorAsync(int vendorId, UpdateVendorRequest vendorUpdateRequest, CancellationToken cancellationToken = default)
    {
        var vendor = await _vendorRepository.GetVendorByIdAsync(vendorId, cancellationToken);

        if(vendor == null)
            throw new NotFoundException<int>(nameof(vendor), vendorId);

        if (!string.IsNullOrWhiteSpace(vendorUpdateRequest.Name))
            vendor.Name = vendorUpdateRequest.Name;

        if (vendorUpdateRequest.IsActive.HasValue)
            vendor.IsActive = vendorUpdateRequest.IsActive.Value;

        vendor.LastUpdatedAtUtc = DateTime.UtcNow;

        await _vendorRepository.UpdateVendorAsync(vendor, cancellationToken);

        return VendorMapper.ToResponse(vendor);


    }

    public Task<VendorInterfaceResponse?> UpdateVendorInterfaceAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
