using InstrumentCatalogue.Application.DTOs.Instrument;
using InstrumentCatalogue.Application.Exceptions;
using InstrumentCatalogue.Application.Extensions;
using InstrumentCatalogue.Application.Mappers;
using InstrumentCatalogue.Core.Enums;
using InstrumentCatalogue.Core.Interfaces;
using InstrumentCatalogue.Core.Models;
using System.Diagnostics.Metrics;

namespace InstrumentCatalogue.Application.Services;

public class InstrumentService : IInstrumentService
{
    private readonly IVendorRepository _vendorRepository;

    private readonly ISymbologyRepository _symbologyRepository;

    private readonly IInstrumentRepository _instrumentRepository;

    private readonly InstrumentMapper _instrumentMapper;

    public InstrumentService(IVendorRepository vendorRepository, ISymbologyRepository symbologyRepository, IInstrumentRepository instrumentRepository, InstrumentMapper instrumentMapper)
    {
        _vendorRepository = vendorRepository ?? throw new ArgumentNullException(nameof(vendorRepository));
        _symbologyRepository = symbologyRepository ?? throw new ArgumentNullException(nameof(symbologyRepository));
        _instrumentRepository = instrumentRepository ?? throw new ArgumentNullException(nameof(instrumentRepository));
        _instrumentMapper = instrumentMapper ?? throw new ArgumentNullException(nameof(instrumentMapper));

    }
    public async Task<Guid> CreateAsync(CreateInstrumentRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var vendorInterface = await _vendorRepository.GetVendorInterfaceByNamesAsync(request.VendorName, request.InterfaceName, cancellationToken);

        if (vendorInterface == null)
            throw new NotFoundException<int>(nameof(VendorInterface), 0, $"{nameof(VendorInterface)} not found for vendor_name={request.VendorName}, interface_name={request.InterfaceName}");

        var symbologyTypeCodesRequest = request.Symbols.Select(s => s.SymbologyTypeCode).ToList();
        var symbologies = await _symbologyRepository.GetSymbologiesByTypeCodeAsync(symbologyTypeCodesRequest, cancellationToken);

        if (symbologies.Count != symbologyTypeCodesRequest.Count)
        {
            var missingSymbologies = symbologyTypeCodesRequest.Except(symbologies.Select(s => s.TypeCode)).ToList();
            var formattedMissingSymbologies = string.Join(",", missingSymbologies);
            throw new NotFoundException<string>(nameof(Symbology), formattedMissingSymbologies, $"Symbologies {formattedMissingSymbologies} not found");

        }

        var instrument = _instrumentMapper.ToDomain(request, symbologies.ToDictionary(s => s.TypeCode, s => s.SymbologyId));
        instrument.StatusHistory = new List<InstrumentStatusHistory>()
        {
            new InstrumentStatusHistory()
            {
                InstrumentStatus = InstrumentStatus.Active,
                ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow),
                EffectiveDate = DateOnly.FromDateTime(DateTime.UtcNow),

            }.StampCreated()};
        
    
        return new Guid();
    
    }
}
