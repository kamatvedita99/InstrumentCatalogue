using InstrumentCatalogue.Application.DTOs;
using InstrumentCatalogue.Application.Exceptions;
using InstrumentCatalogue.Application.Extensions;
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

        var vendor = await _vendorRepository.GetVendorByIdAsync(vendorId, cancellationToken);
        
        if(vendor == null)
            throw new NotFoundException<int>(nameof(Vendor), vendorId);

        var vendorInterface = VendorInterfaceMapper.ToDomain(vendorId, request);
        await _vendorRepository.CreateVendorInterfaceAsync(vendorInterface, cancellationToken);

        return VendorInterfaceMapper.ToResponse(vendorInterface);
    }

    public async Task<VendorResponse?> GetVendorByIdAsync(int vendorId, CancellationToken cancellationToken = default)
    {
       var vendor = await _vendorRepository.GetVendorByIdAsync(vendorId, cancellationToken);
       
        if(vendor == null) 
            throw new NotFoundException<int>(nameof(Vendor), vendorId);
        
        return VendorMapper.ToResponse(vendor);
    }

    public async Task<VendorInterfaceResponse?> GetVendorInterfaceByIdAsync(int vendorId, int vendorInterfaceId, CancellationToken cancellationToken = default)
    {
        var vendorInterface = await _vendorRepository.GetVendorInterfaceByIdAsync(vendorId, vendorInterfaceId, cancellationToken);
        
        if(vendorInterface == null)
            throw new NotFoundException<int>(nameof(VendorInterface), vendorInterfaceId, $"{nameof(VendorInterface)} with Id: {vendorInterfaceId} was not found for {nameof(Vendor)} with Id: {vendorId}.");

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
        ArgumentNullException.ThrowIfNull(vendorUpdateRequest);

        var vendor = await _vendorRepository.GetVendorByIdAsync(vendorId, cancellationToken);

        if(vendor == null)
            throw new NotFoundException<int>(nameof(Vendor), vendorId);

        if (!string.IsNullOrWhiteSpace(vendorUpdateRequest.Name))
            vendor.Name = vendorUpdateRequest.Name;

        if (vendorUpdateRequest.IsActive.HasValue)
            vendor.IsActive = vendorUpdateRequest.IsActive.Value;

        vendor.StampUpdated();

        await _vendorRepository.UpdateVendorAsync(vendor, cancellationToken);

        return VendorMapper.ToResponse(vendor);


    }

    public async Task<VendorInterfaceResponse?> UpdateVendorInterfaceAsync(int vendorId, int vendorInterfaceId, UpdateVendorInterfaceRequest request, CancellationToken cancellationToken = default)
    {
        var vendorInterface = await _vendorRepository.GetVendorInterfaceByIdAsync(vendorId, vendorInterfaceId, cancellationToken);

        if(vendorInterface == null)
            throw new NotFoundException<int>(nameof(VendorInterface), vendorInterfaceId, $"{nameof(VendorInterface)} with Id: {vendorInterfaceId} was not found for {nameof(Vendor)} with Id: {vendorId}.");

        if(!string.IsNullOrWhiteSpace(request.Name))
            vendorInterface.Name = request.Name;

        if(!string.IsNullOrWhiteSpace(request.Description))
            vendorInterface.Description = request.Description;

        if(!string.IsNullOrWhiteSpace(request.Protocol))
            vendorInterface.Protocol = request.Protocol;

        if(request.IsActive.HasValue)
            vendorInterface.IsActive = request.IsActive.Value;

        vendorInterface.StampUpdated();

        await _vendorRepository.UpdateVendorInterfaceAsync(vendorInterface, cancellationToken);
        return VendorInterfaceMapper.ToResponse(vendorInterface);
        
    }
}
