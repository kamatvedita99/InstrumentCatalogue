using FluentValidation;
using InstrumentCatalogue.API.ReadModels;
using InstrumentCatalogue.Application.DTOs.Instrument;
using InstrumentCatalogue.Application.DTOs.InstrumentStatusHistory;
using InstrumentCatalogue.Application.DTOs.SymbolXRef;
using InstrumentCatalogue.Application.Services;
using InstrumentCatalogue.Core.Cache;
using InstrumentCatalogue.Core.Common;
using InstrumentCatalogue.Core.Filters;
using InstrumentCatalogue.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace InstrumentCatalogue.API.Controllers;

[ApiController]
[Route("api/v1/instruments")]
public class InstrumentController : ControllerBase
{
    private readonly IInstrumentService _instrumentService;

    private readonly ILogger<InstrumentController> _logger;

    public InstrumentController(IInstrumentService instrumentService, ILogger<InstrumentController> logger)
    {
        _instrumentService = instrumentService ?? throw new ArgumentNullException(nameof(instrumentService));
        _logger = logger ?? throw new ArgumentNullException(nameof(_logger));
    }
    [HttpPost]
    public async Task<ActionResult<ApiResponse<InstrumentResponse>>> CreateAsync([FromBody]CreateInstrumentRequest request, IValidator<CreateInstrumentRequest> validator, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var instrumentResponse = await _instrumentService.CreateAsync(request, cancellationToken);

        return Created(string.Empty, ApiResponse<InstrumentResponse>.Success(instrumentResponse));
    }

    [HttpGet]
    [Route("{id}")]
    public async Task<ActionResult<ApiResponse<InstrumentResponse?>>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var instrumentResponse = await _instrumentService.GetByIdAsync(id, cancellationToken);

        return Ok(ApiResponse<InstrumentResponse?>.Success(instrumentResponse));
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult<ApiResponse<InstrumentResponse?>>> UpdateAsync(Guid id, [FromBody] UpdateInstrumentRequest request, IValidator<UpdateInstrumentRequest> validator, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var instrumentResponse = await _instrumentService.UpdateAsync(id, request, cancellationToken);

        return Ok(ApiResponse<InstrumentResponse?>.Success(instrumentResponse));
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<InstrumentResponse>>> GetAsync([FromQuery] PagedRequest<InstrumentFilter> pagedRequest, IValidator<InstrumentFilter> validator, CancellationToken cancellationToken = default)
    {
        pagedRequest.Filter ??= new InstrumentFilter();
        await validator.ValidateAndThrowAsync(pagedRequest.Filter, cancellationToken);
        var pagedResponse = await _instrumentService.GetAllAsync(pagedRequest, cancellationToken);

        return Ok(ApiResponse<PagedResult<InstrumentResponse>>.Success(pagedResponse));
    }

    [HttpGet("resolve")]
    public async Task<ActionResult<ApiResponse<ResolvedSymbol?>>> ResolveSymbolAsync([FromQuery]string typecode, [FromQuery]string symbol, CancellationToken cancellationToken = default)
    {
       
        var resolvedSymbol = await _instrumentService.ResolveSymbolAsync(typecode, symbol, cancellationToken);
        return Ok(ApiResponse<ResolvedSymbol?>.Success(resolvedSymbol));

    }

    [HttpPost("{id}/symbols")]
    public async Task<ActionResult<ApiResponse<SymbolXRefResponse?>>> CreateSymbolAsync(Guid id, [FromBody] CreateInstrumentSymbolRequest createSymbolRequest, IValidator<CreateInstrumentSymbolRequest> validator, CancellationToken cancellationToken = default)    
    {
        await validator.ValidateAndThrowAsync(createSymbolRequest, cancellationToken);

       var createdSymbol =  await _instrumentService.CreateSymbolAsync(id, createSymbolRequest, cancellationToken);
        
       if(createdSymbol == null)
            return NoContent();
        
       return Created(string.Empty, ApiResponse<SymbolXRefResponse>.Success(createdSymbol));
    }

    [HttpPatch("{id}/symbols/{symbolId}")]
    public async Task<ActionResult<ApiResponse<SymbolXRefResponse?>>> UpdateSymbolAsync(Guid id, Guid symbolId, [FromBody] UpdateSymbolXRefRequest? updateSymbolRequest, CancellationToken cancellationToken = default)
    {
        var updatedSymbol = await _instrumentService.UpdateSymbolAsync(symbolId, id, updateSymbolRequest, cancellationToken);

        if(updatedSymbol == null)
            return NoContent();

        return Ok(ApiResponse<SymbolXRefResponse>.Success(updatedSymbol));
    }

    [HttpGet("{id}/status-history")]
    public async Task<ActionResult<ApiResponse<ICollection<InstrumentStatusHistoryResponse>>>> GetInstrumentStatusHistoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
       var instrumentStatusHistoryList = await _instrumentService.GetInstrumentStatusHistoryAsync(id, cancellationToken);

        return Ok(ApiResponse<ICollection<InstrumentStatusHistoryResponse>>.Success(instrumentStatusHistoryList));
    }

    [HttpPost("{id}/status-history")]
    public async Task<ActionResult<ApiResponse<InstrumentStatusHistoryResponse?>>> UpdateInstrumentStatusAsync(Guid id, [FromBody]UpdateInstrumentStatusHistoryRequest request, IValidator<UpdateInstrumentStatusHistoryRequest> validator, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        var instrumentStatusResponse = await _instrumentService.UpdateInstrumentStatusAsync(id, request, cancellationToken);

        if (instrumentStatusResponse is null)
            return NoContent();

        return Ok(ApiResponse<InstrumentStatusHistoryResponse>.Success(instrumentStatusResponse));
    }

}
