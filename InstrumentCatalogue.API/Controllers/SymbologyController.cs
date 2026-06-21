using FluentValidation;
using InstrumentCatalogue.API.ReadModels;
using InstrumentCatalogue.Application.DTOs.Symbology;
using InstrumentCatalogue.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace InstrumentCatalogue.API.Controllers;

[ApiController]
[Route("api/v1/symbologies")]
public class SymbologyController: ControllerBase
{
    private readonly ISymbologyService _symbologyService;

    public SymbologyController(ISymbologyService symbologyService)
    {
        _symbologyService = symbologyService ?? throw new ArgumentNullException(nameof(symbologyService));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<SymbologyResponse>>> CreateSymbologyAsync([FromBody] CreateSymbologyRequest symbologyRequest, IValidator<CreateSymbologyRequest> validator, CancellationToken cancellationToken = default)
    {
       await validator.ValidateAndThrowAsync(symbologyRequest, cancellationToken);
       var response = await _symbologyService.CreateSymbologyAsync(symbologyRequest, cancellationToken);
       return Created(string.Empty, ApiResponse<SymbologyResponse>.Success(response));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<ICollection<SymbologyResponse>>>> GetSymbologiesAsync(CancellationToken cancellationToken = default)
    {
        var response = await _symbologyService.GetSymbologiesAsync(cancellationToken);
        return Ok(ApiResponse<ICollection<SymbologyResponse>>.Success(response));
            
    }

    [HttpGet]
    [Route("{id}")]
    public async Task<ActionResult<ApiResponse<SymbologyResponse?>>> GetSymbologyByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await _symbologyService.GetSymbologyByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<SymbologyResponse?>.Success(response));
    }

    [HttpPatch]
    [Route("{id}")]
    public async Task<ActionResult<ApiResponse<SymbologyResponse?>>> UpdateSymbologyAsync(int id, [FromBody] UpdateSymbologyRequest request, IValidator<UpdateSymbologyRequest> validator, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var response = await _symbologyService.UpdateSymbologyAsync(id, request, cancellationToken);
        return Ok(ApiResponse<SymbologyResponse?>.Success(response));
    }

}
