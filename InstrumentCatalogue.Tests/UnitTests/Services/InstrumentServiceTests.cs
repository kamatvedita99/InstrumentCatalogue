using InstrumentCatalogue.Application.Exceptions;
using InstrumentCatalogue.Application.Services;
using InstrumentCatalogue.Core.Cache;
using InstrumentCatalogue.Core.Enums;
using InstrumentCatalogue.Core.Interfaces;
using InstrumentCatalogue.Core.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;

public class InstrumentServiceTests
{
    private readonly ISymbologyRepository _symbologyRepository;
    private readonly IVendorRepository _vendorRepository;
    private readonly IInstrumentRepository _instrumentRepository;
    private readonly ISymbologyCache _symbologyCache;
    private readonly ISymbolResolutionCache _symbolResolutionCache;
    private readonly ILogger<InstrumentService> _logger;
    private readonly InstrumentService _sut; // sut = system under test

    public InstrumentServiceTests()
    {
        // create mocks and construct the service
        _instrumentRepository = Substitute.For<IInstrumentRepository>();
        _vendorRepository = Substitute.For<IVendorRepository>();
        _symbologyRepository = Substitute.For<ISymbologyRepository>();
        _symbologyCache = Substitute.For<ISymbologyCache>();
        _symbolResolutionCache = Substitute.For<ISymbolResolutionCache>();
        _logger = Substitute.For<ILogger<InstrumentService>>();

        _sut = new InstrumentService(_vendorRepository, _symbologyRepository, _instrumentRepository,  
            _symbologyCache, _symbolResolutionCache, _logger);


    }

    [Fact]
    public async Task TestSymbologySymbolCacheHit()
    {
        var symbologyTypeCode = "ISIN";
        var symbologyId = 2;
        var symbol = "INE467B01029";
        var instrumentName = "Tata Consultancy Services Limited";

        _symbologyCache.TryGet(symbologyTypeCode, out Arg.Any<int>())
            .Returns(x => { x[1] = symbologyId; return true; });

         _symbolResolutionCache.GetAsync(symbologyId, symbol, Arg.Any<CancellationToken>())
            .Returns(new ResolvedSymbol()
            {
                InstrumentId = Guid.Parse("51b70521-1e73-4bef-b27f-f8ea80da765a"),
                Type = InstrumentType.Equity,
                Name = instrumentName
            });
        var actualResult = await _sut.ResolveSymbolAsync(symbologyTypeCode, symbol, default);

        await _symbologyRepository.DidNotReceive().GetSymbologiesByTypeCodeAsync(Arg.Any<ICollection<string>>(), Arg.Any<CancellationToken>());
        await _instrumentRepository.DidNotReceive().ResolveSymbolAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        Assert.NotNull(actualResult);
        Assert.Equal(instrumentName, actualResult?.Name);
    }

    [Fact]
    public async Task TestSymbologyCacheHitSymbolCacheMiss()
    {
        var symbologyTypeCode = "ISIN";
        var symbologyId = 2;
        var symbol = "INE467B01029";
        var instrumentName = "Tata Consultancy Services Limited";

        _symbologyCache.TryGet(symbologyTypeCode, out Arg.Any<int>())
            .Returns(x => { x[1] = symbologyId; return true; });

        _symbolResolutionCache.GetAsync(symbologyId, symbol, Arg.Any<CancellationToken>())
           .Returns((ResolvedSymbol?)null);
        
        _instrumentRepository.ResolveSymbolAsync(symbologyTypeCode, symbol, Arg.Any<CancellationToken>())
            .Returns(new ResolvedSymbol()
            {
            InstrumentId = Guid.Parse("51b70521-1e73-4bef-b27f-f8ea80da765a"),
                Type = InstrumentType.Equity,
                Name = instrumentName
            });

        var actualResult = await _sut.ResolveSymbolAsync(symbologyTypeCode, symbol, default);

        await _symbologyRepository.DidNotReceive().GetSymbologiesByTypeCodeAsync(Arg.Any<ICollection<string>>(), Arg.Any<CancellationToken>());

        await _instrumentRepository.Received().ResolveSymbolAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _symbolResolutionCache.Received(1).SetAsync(symbologyId, symbol, Arg.Any<ResolvedSymbol>(), Arg.Any<CancellationToken>());

        Assert.NotNull(actualResult);
        Assert.Equal(instrumentName, actualResult?.Name);
    }

    [Fact]
    public async Task TestSymbologyCacheMissSymbolCacheHit()
    {
        var symbologyTypeCode = "ISIN";
        var symbologyId = 2;
        var symbol = "INE467B01029";
        var instrumentName = "Tata Consultancy Services Limited";

        _symbologyCache.TryGet(symbologyTypeCode, out Arg.Any<int>())
            .Returns(false);

        _symbologyRepository.GetSymbologiesByTypeCodeAsync(Arg.Is<ICollection<string>>(list => list.Contains(symbologyTypeCode)), Arg.Any<CancellationToken>())
            .Returns(new List<Symbology>{new()
            {
                SymbologyId = symbologyId,
                TypeCode = symbologyTypeCode,
            }});

        _symbolResolutionCache.GetAsync(symbologyId, symbol, Arg.Any<CancellationToken>())
           .Returns(new ResolvedSymbol()
           {
               InstrumentId = Guid.Parse("51b70521-1e73-4bef-b27f-f8ea80da765a"),
               Type = InstrumentType.Equity,
               Name = instrumentName
           });  

     
        var actualResult = await _sut.ResolveSymbolAsync(symbologyTypeCode, symbol, default);

        await _symbologyRepository.Received(1).GetSymbologiesByTypeCodeAsync(Arg.Any<ICollection<string>>(), Arg.Any<CancellationToken>());
        _symbologyCache.Received(1).Set(symbologyTypeCode,symbologyId);

        await _instrumentRepository.DidNotReceive().ResolveSymbolAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        Assert.NotNull(actualResult);
        Assert.Equal(instrumentName, actualResult?.Name);
    }

    [Fact]
    public async Task TestSymbologySymbolCacheMiss()
    {
        
        var symbologyTypeCode = "ISIN";
        var symbologyId = 2;
        var symbol = "INE467B01029";
        var instrumentName = "Tata Consultancy Services Limited";
        var resolvedSymbol = new ResolvedSymbol()
        {
            InstrumentId = Guid.Parse("51b70521-1e73-4bef-b27f-f8ea80da765a"),
            Type = InstrumentType.Equity,
            Name = instrumentName
        };

        _symbologyCache.TryGet(symbologyTypeCode, out Arg.Any<int>())
            .Returns(false);

        _symbolResolutionCache.GetAsync(symbologyId, symbol, Arg.Any<CancellationToken>())
           .Returns((ResolvedSymbol?)null);

        _symbologyRepository.GetSymbologiesByTypeCodeAsync(Arg.Is<ICollection<string>>(list => list.Contains(symbologyTypeCode)), Arg.Any<CancellationToken>())
            .Returns(new List<Symbology>{new()
            {
                SymbologyId = symbologyId,
                TypeCode = symbologyTypeCode,
            }});

        _instrumentRepository.ResolveSymbolAsync(symbologyTypeCode, symbol, Arg.Any<CancellationToken>())
            .Returns(resolvedSymbol);

        var actualResult = await _sut.ResolveSymbolAsync(symbologyTypeCode, symbol, default);

        await _symbologyRepository.Received(1).GetSymbologiesByTypeCodeAsync(Arg.Any<ICollection<string>>());
        _symbologyCache.Received(1).Set(symbologyTypeCode, symbologyId);

        await _instrumentRepository.Received(1).ResolveSymbolAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _symbolResolutionCache.Received(1).SetAsync(symbologyId, symbol, resolvedSymbol, default);

        Assert.NotNull(actualResult);
        Assert.Equal(instrumentName, actualResult?.Name);
    }

    [Fact]
    public async Task TestSymbologyNotFound()
    {
        var symbologyTypeCode = "ISINfake";
        var symbol = "INE467B01029";

        _symbologyCache.TryGet(symbologyTypeCode, out Arg.Any<int>())
           .Returns(false);

        _symbologyRepository.GetSymbologiesByTypeCodeAsync(Arg.Is<ICollection<string>>(list => list.Contains(symbologyTypeCode)), Arg.Any<CancellationToken>())
            .Returns(new List<Symbology>{});

        await Assert.ThrowsAnyAsync<NotFoundException>(async () => await _sut.ResolveSymbolAsync(symbologyTypeCode, symbol, default));
        await _symbologyRepository.Received(1).GetSymbologiesByTypeCodeAsync(Arg.Any<ICollection<string>>(), Arg.Any<CancellationToken>());
        await _instrumentRepository.DidNotReceive().ResolveSymbolAsync(symbologyTypeCode, Arg.Any<string>(), Arg.Any<CancellationToken>());

    }

    [Fact]
    public async Task TestSymbolNotFound()
    {
        var symbologyTypeCode = "ISIN";
        var symbologyId = 2;
        var symbol = "INE467B01028";

        _symbologyCache.TryGet(symbologyTypeCode, out Arg.Any<int>())
           .Returns(x => { x[1] = symbologyId; return true; });

        _symbolResolutionCache.GetAsync(symbologyId, symbol, Arg.Any<CancellationToken>())
           .Returns((ResolvedSymbol?)null);

        _instrumentRepository.ResolveSymbolAsync(symbologyTypeCode, symbol, Arg.Any<CancellationToken>())
           .Returns((ResolvedSymbol?)null);

        await Assert.ThrowsAnyAsync<NotFoundException>(async ()=> await _sut.ResolveSymbolAsync(symbologyTypeCode, symbol, default));
        await _instrumentRepository.Received(1).ResolveSymbolAsync(symbologyTypeCode, symbol, default);

    }
}