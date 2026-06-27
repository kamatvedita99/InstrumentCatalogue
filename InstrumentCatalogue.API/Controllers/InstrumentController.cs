using FluentValidation;
using InstrumentCatalogue.API.ReadModels;
using InstrumentCatalogue.Application.DTOs.Instrument;
using InstrumentCatalogue.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace InstrumentCatalogue.API.Controllers;

[ApiController]
[Route("api/v1/instruments")]
public class InstrumentController : ControllerBase
{
    private readonly IInstrumentService _instrumentService;

    public InstrumentController(IInstrumentService instrumentService)
    {
        _instrumentService = instrumentService ?? throw new ArgumentNullException(nameof(instrumentService));
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
}
