using InstrumentCatalogue.Application.DTOs.Instrument;
using InstrumentCatalogue.Core.Interfaces;
using System.Diagnostics.Metrics;

namespace InstrumentCatalogue.Application.Services;

public class InstrumentService : IInstrumentService
{
    private readonly IVendorRepository _vendorRepository;

    private readonly ISymbologyRepository _symbologyRepository;

    private readonly IInstrumentRepository _instrumentRepository;

    public InstrumentService(IVendorRepository vendorRepository, ISymbologyRepository symbologyRepository, IInstrumentRepository instrumentRepository)
    {
        _vendorRepository = vendorRepository;
        _symbologyRepository = symbologyRepository;
        _instrumentRepository = instrumentRepository;
    }
    public Task<Guid> CreateAsync(CreateInstrumentRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
