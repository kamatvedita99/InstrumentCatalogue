using InstrumentCatalogue.Application.DTOs.Instrument;
using InstrumentCatalogue.Application.DTOs.SymbolXRef;
using InstrumentCatalogue.Application.Exceptions;
using InstrumentCatalogue.Application.Extensions;
using InstrumentCatalogue.Application.Mappers;
using InstrumentCatalogue.Core.Cache;
using InstrumentCatalogue.Core.Common;
using InstrumentCatalogue.Core.Enums;
using InstrumentCatalogue.Core.Filters;
using InstrumentCatalogue.Core.Interfaces;
using InstrumentCatalogue.Core.Models;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Threading;

namespace InstrumentCatalogue.Application.Services;


public class InstrumentService : IInstrumentService
{
    private readonly IVendorRepository _vendorRepository;

    private readonly ISymbologyRepository _symbologyRepository;

    private readonly IInstrumentRepository _instrumentRepository;

    private readonly ISymbologyCache _symbologyCache;

    private readonly ISymbolResolutionCache _symbolResolutionCache;

    private readonly ILogger<InstrumentService> _logger;

    public InstrumentService(IVendorRepository vendorRepository, ISymbologyRepository symbologyRepository, IInstrumentRepository instrumentRepository, ISymbologyCache symbologyCache, ISymbolResolutionCache symbolResolutionCache, ILogger<InstrumentService> logger)
    {
        _vendorRepository = vendorRepository ?? throw new ArgumentNullException(nameof(vendorRepository));
        _symbologyRepository = symbologyRepository ?? throw new ArgumentNullException(nameof(symbologyRepository));
        _instrumentRepository = instrumentRepository ?? throw new ArgumentNullException(nameof(instrumentRepository));
        _symbologyCache = symbologyCache ?? throw new ArgumentNullException(nameof(symbologyCache));
        _symbolResolutionCache = symbolResolutionCache ?? throw new ArgumentNullException(nameof(symbolResolutionCache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));


    }

    private async Task<Dictionary<string, int>> GetSymbologyMapping(CreateInstrumentRequest request, CancellationToken cancellationToken)
    {
        var symbologyTypeCodesRequest = request.Symbols.Select(s => s.SymbologyTypeCode).ToList();
        var symbologyMap = new Dictionary<string, int>();
        var missingSymbologiesFromCache = new List<string>();
        
        //get from cache
        foreach(var symbolTypeCode in symbologyTypeCodesRequest)
        {
            if(_symbologyCache.TryGet(symbolTypeCode, out int symbologyId))
            {
                symbologyMap[symbolTypeCode] = symbologyId;
            }

            else
            {
                missingSymbologiesFromCache.Add(symbolTypeCode.ToString());
            }
        }

        
        if (symbologyMap.Count == symbologyTypeCodesRequest.Count)
            return symbologyMap;

        //get from db
        var symbologies = await _symbologyRepository.GetSymbologiesByTypeCodeAsync(missingSymbologiesFromCache, cancellationToken);
        foreach (var symbology in symbologies)
        {
            symbologyMap[symbology.TypeCode] = symbology.SymbologyId;
            _symbologyCache.Set(symbology.TypeCode, symbology.SymbologyId);
        }
        

        if (symbologyMap.Count != symbologyTypeCodesRequest.Count)
        {
            var missingSymbologies = symbologyTypeCodesRequest.Except(symbologyMap.Keys.ToList()).ToList();
            var formattedMissingSymbologies = string.Join(",", missingSymbologies);
            throw new NotFoundException<string>(nameof(Symbology), formattedMissingSymbologies, $"Symbologies {formattedMissingSymbologies} not found");

        }

        return symbologyMap;

    }

    private async Task<int> GetSymbologyIdAsync(string symbology, CancellationToken cancellationToken)
    {
        _logger.LogTrace("Fetching symbology");
        var isSymbologyCached = _symbologyCache.TryGet(symbology, out var symbologyId);
        if (!isSymbologyCached)
        {
            _logger.LogDebug("Cache miss for symbology {symbology}", symbology);

            var symbologyDetail = (await _symbologyRepository.GetSymbologiesByTypeCodeAsync(new List<string> { symbology }, cancellationToken)).SingleOrDefault();

            if (symbologyDetail is null)
                throw new NotFoundException<string>(nameof(Symbology), $"Could not find symbology with type code: {symbology}");

            symbologyId = symbologyDetail.SymbologyId;
            _symbologyCache.Set(symbology, symbologyId);

        }
        return symbologyId;
    }
    public async Task<InstrumentResponse> CreateAsync(CreateInstrumentRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var vendorInterface = await _vendorRepository.GetVendorInterfaceByNamesAsync(request.VendorName, request.InterfaceName, cancellationToken);

        if (vendorInterface == null)
            throw new NotFoundException<string>(nameof(VendorInterface), $"{nameof(VendorInterface)} not found for vendor_name={request.VendorName}, interface_name={request.InterfaceName}");


        var symbologyMap = await GetSymbologyMapping(request, cancellationToken);

        var instrument = InstrumentMapper.ToDomain(request, symbologyMap);
        instrument.StatusHistory = new List<InstrumentStatusHistory>()
        {
            new InstrumentStatusHistory()
            {
                InstrumentStatus = InstrumentStatus.Active,
                ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow),
                EffectiveDate = DateOnly.FromDateTime(DateTime.UtcNow),

            }.StampCreated()};

        await _instrumentRepository.CreateAsync(instrument, vendorInterface.VendorInterfaceId, cancellationToken);

        foreach(var symbol in instrument.Symbols)
        {
            await _symbolResolutionCache.SetAsync(symbol.SymbologyId, symbol.Symbol, new ResolvedSymbol { InstrumentId = instrument.InstrumentId, Name = instrument.Name, Type = instrument.Type }, cancellationToken);
        }
        return InstrumentMapper.ToResponse(instrument);
    
    }

    public async Task<PagedResult<InstrumentResponse>> GetAllAsync(PagedRequest<InstrumentFilter> pagedRequest, CancellationToken cancellationToken = default)
    {
        var pagedResponse = await _instrumentRepository.GetAsync(pagedRequest, cancellationToken);

        return new PagedResult<InstrumentResponse> {
            NextCursor = pagedResponse.NextCursor,
            Items = pagedResponse.Items.Select(InstrumentMapper.ToResponse).ToList() ?? new List<InstrumentResponse>()
            };
    }

    public async Task<InstrumentResponse?> GetByIdAsync(Guid instrumentId, CancellationToken cancellationToken = default)
    {
        var instrument = await _instrumentRepository.GetByIdAsync(instrumentId, cancellationToken);
        
        if(instrument is null)
            throw new NotFoundException<Guid>(nameof(Instrument), instrumentId);

        return InstrumentMapper.ToResponse(instrument);
    }

    public async Task<ResolvedSymbol?> ResolveSymbolAsync(string symbology, string symbol, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(symbol);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(symbology);

        var symbologyId = await GetSymbologyIdAsync(symbology, cancellationToken);
        
        _logger.LogTrace("Fetching symbol");
        var resolvedSymbolCache = await _symbolResolutionCache.GetAsync(symbologyId, symbol, cancellationToken);
        var resolvedSymbol =  resolvedSymbolCache ?? await _instrumentRepository.ResolveSymbolAsync(symbology, symbol, cancellationToken);

        if (resolvedSymbol is null)
            throw new NotFoundException<string>(nameof(SymbolXRef), $"Could not find resolution for symbology:{symbology} & symbol:{symbol}");

        if (resolvedSymbolCache is null)
        {
            await _symbolResolutionCache.SetAsync(symbologyId, symbol, resolvedSymbol, cancellationToken);
            _logger.LogDebug("Cache miss for symbology {symbology} symbol {symbol}", symbology, symbol);
        }

            return resolvedSymbol;

    }

    public async Task<SymbolXRefResponse> CreateSymbolAsync(Guid instrumentId, CreateInstrumentSymbolRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var symbologyId = await GetSymbologyIdAsync(request.SymbologyTypeCode, cancellationToken);

        var symbol = SymbolXRefMapper.ToDomain(instrumentId, symbologyId, request);

        await _instrumentRepository.CreateSymbolAsync(symbol, cancellationToken);

        return SymbolXRefMapper.ToResponse(symbol);
    }
}
