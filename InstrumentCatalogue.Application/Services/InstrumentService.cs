using InstrumentCatalogue.Application.DTOs.Instrument;
using InstrumentCatalogue.Application.DTOs.InstrumentStatusHistory;
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
using InstrumentCatalogue.Core.Rules;
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

        var vendorInterfaceSymbolXRefs = instrument.Symbols.Select(symbol => new VendorInterfaceSymbolXRef
        {
            SymbolXRefId = symbol.SymbolXRefId,
            VendorInterfaceId = vendorInterface.VendorInterfaceId,
            ReceivedAtUtc = DateTime.UtcNow
        }.StampCreated()).ToList();


        await _instrumentRepository.CreateAsync(instrument, vendorInterfaceSymbolXRefs, cancellationToken);

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

    public async Task<SymbolXRefResponse?> CreateSymbolAsync(Guid instrumentId, CreateInstrumentSymbolRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var symbologyId = await GetSymbologyIdAsync(request.SymbologyTypeCode, cancellationToken);

        var symbol = SymbolXRefMapper.ToDomain(instrumentId, symbologyId, request);

        var existingSymbol = await _instrumentRepository.GetActiveSymbolAsync(symbol.InstrumentId, symbol.SymbologyId, cancellationToken);

        if(existingSymbol is not null)
        {
            if(existingSymbol.Symbol == symbol.Symbol)
                return null;
            
            existingSymbol.ValidTo = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1);
            existingSymbol.StampUpdated();


        }

        await _instrumentRepository.CreateSymbolAsync(symbol, existingSymbol, cancellationToken);

        return SymbolXRefMapper.ToResponse(symbol);
    }

    public async Task<ICollection<InstrumentStatusHistoryResponse>> GetInstrumentStatusHistoryAsync(Guid instrumentId, CancellationToken cancellationToken = default)
    {
        var instrumentStatusHistoryList = await _instrumentRepository.GetStatusHistoryAsync(instrumentId, cancellationToken);

        return instrumentStatusHistoryList.Select(InstrumentStatusHistoryMapper.ToResponse).ToList();
    }

    public async Task<InstrumentStatusHistoryResponse?> UpdateInstrumentStatusAsync(Guid instrumentId, UpdateInstrumentStatusHistoryRequest request, CancellationToken cancellationToken = default)
    {
        var existingInstrumentStatus = await _instrumentRepository.GetActiveStatusHistoryAsync(instrumentId, cancellationToken);

        if(existingInstrumentStatus is null)
            throw new NotFoundException<Guid>(nameof(InstrumentStatusHistory), $"Instrument {instrumentId} status details not found");

        if(existingInstrumentStatus.InstrumentStatus == request.InstrumentStatus)
            return null;

        InstrumentStatusTransitionRule.ValidateTransition(existingInstrumentStatus.InstrumentStatus, request.InstrumentStatus);

        existingInstrumentStatus.ValidTo = DateOnly.FromDateTime(DateTime.UtcNow);
        existingInstrumentStatus.StampUpdated();

        var instrumentStatus = InstrumentStatusHistoryMapper.ToDomain(instrumentId, request);

        await _instrumentRepository.UpdateStatusAsync(instrumentStatus, existingInstrumentStatus, cancellationToken);
        return InstrumentStatusHistoryMapper.ToResponse(instrumentStatus);
        
    }

    public async Task<InstrumentResponse?> UpdateAsync(Guid instrumentId, UpdateInstrumentRequest updateInstrumentRequest, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(updateInstrumentRequest);

        var instrument = await _instrumentRepository.GetByIdAsync(instrumentId, cancellationToken);

        if(instrument is null)
            throw new NotFoundException<Guid>(nameof(Instrument), instrumentId);

        if(!string.IsNullOrWhiteSpace(updateInstrumentRequest.Name))
            instrument.Name = updateInstrumentRequest.Name;

        if(!string.IsNullOrWhiteSpace(updateInstrumentRequest.Currency))
            instrument.Currency = updateInstrumentRequest.Currency;

        if(!string.IsNullOrWhiteSpace(updateInstrumentRequest.Country))
            instrument.Country = updateInstrumentRequest.Country;

        if(!string.IsNullOrWhiteSpace(updateInstrumentRequest.Exchange))
            instrument.Exchange = updateInstrumentRequest.Exchange;

        if(updateInstrumentRequest.ListedDate.HasValue)
            instrument.ListedDate = updateInstrumentRequest.ListedDate.Value;

        switch(instrument.Type)
        {
            case InstrumentType.Bond:
                UpdateBondRefData(instrument, updateInstrumentRequest.BondRef);
                break;

            case InstrumentType.Equity:
                UpdateEquityRefData(instrument, updateInstrumentRequest.EquityRef);
                break;

            case InstrumentType.ETF:
                UpdateEtfRefData(instrument, updateInstrumentRequest.EtfRef);
                break;
        }

        instrument.StampUpdated();
        await _instrumentRepository.UpdateAsync(instrument, cancellationToken);

        if (!string.IsNullOrWhiteSpace(updateInstrumentRequest.Name))
        {
            foreach (var symbol in instrument.Symbols)
            {
                await _symbolResolutionCache.SetAsync(symbol.SymbologyId, symbol.Symbol, new ResolvedSymbol { InstrumentId = instrument.InstrumentId, Name = instrument.Name, Type = instrument.Type }, cancellationToken);
            }
        }
        return InstrumentMapper.ToResponse(instrument);




    }
    private void UpdateBondRefData(Instrument instrument, UpdateBondRefRequest? request)
    {
        if (request is null || instrument.BondRefData is null) return;
        if (request.FaceValue.HasValue) instrument.BondRefData.FaceValue = request.FaceValue.Value;
        if (request.CouponRate.HasValue) instrument.BondRefData.CouponRate = request.CouponRate.Value;
        if (request.CouponFrequency.HasValue) instrument.BondRefData.CouponFrequency = request.CouponFrequency.Value;
        if (!string.IsNullOrWhiteSpace(request.Issuer)) instrument.BondRefData.Issuer = request.Issuer;
        if (request.IssueDate.HasValue) instrument.BondRefData.IssueDate = request.IssueDate;
        if (request.MaturityDate.HasValue) instrument.BondRefData.MaturityDate = request.MaturityDate;
        if (!string.IsNullOrWhiteSpace(request.CreditRating)) instrument.BondRefData.CreditRating = request.CreditRating;
        if (request.BondType.HasValue) instrument.BondRefData.BondType = request.BondType.Value;
        if (request.BondStructure.HasValue) instrument.BondRefData.BondStructure = request.BondStructure.Value;
        if (request.Duration.HasValue) instrument.BondRefData.Duration = request.Duration.Value;
        instrument.BondRefData.StampUpdated();
    }

    private void UpdateEquityRefData(Instrument instrument, UpdateEquityRefRequest? request)
    {
        if (request is null || instrument.EquityRefData is null) return;
        if (!string.IsNullOrWhiteSpace(request.Sector)) instrument.EquityRefData.Sector = request.Sector;
        if (!string.IsNullOrWhiteSpace(request.Industry)) instrument.EquityRefData.Industry = request.Industry;
        if (request.SharesOutstanding.HasValue) instrument.EquityRefData.SharesOutstanding = request.SharesOutstanding.Value;
        if (request.LotSize.HasValue) instrument.EquityRefData.LotSize = request.LotSize.Value;
        if (request.ParValue.HasValue) instrument.EquityRefData.ParValue = request.ParValue.Value;
        instrument.EquityRefData.StampUpdated();
    }

    private void UpdateEtfRefData(Instrument instrument, UpdateEtfRefRequest? request)
    {
        if (request is null || instrument.EtfRefData is null) return;
        if (!string.IsNullOrWhiteSpace(request.FundManager)) instrument.EtfRefData.FundManager = request.FundManager;
        if (!string.IsNullOrWhiteSpace(request.UnderlyingIndex)) instrument.EtfRefData.UnderlyingIndex = request.UnderlyingIndex;
        if (request.ReplicationType.HasValue) instrument.EtfRefData.ReplicationType = request.ReplicationType.Value;
        if (request.DistributionFrequency.HasValue) instrument.EtfRefData.DistributionFrequency = request.DistributionFrequency.Value;
        if (request.InceptionDate.HasValue) instrument.EtfRefData.InceptionDate = request.InceptionDate;
        if (request.ExpenseRatio.HasValue) instrument.EtfRefData.ExpenseRatio = request.ExpenseRatio.Value;
        instrument.EtfRefData.StampUpdated();
    }

}
