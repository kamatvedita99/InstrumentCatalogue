using InstrumentCatalogue.API.ReadModels;
using InstrumentCatalogue.Application.DTOs;
using InstrumentCatalogue.Application.Services;
using InstrumentCatalogue.Core.Models;
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
    public async Task<ActionResult<ApiResponse<SymbologyResponse>>> CreateSymbologyAsync([FromBody] CreateSymbologyRequest symbologyRequest, CancellationToken cancellationToken = default)
    {
       var response = await _symbologyService.CreateSymbologyAsync(symbologyRequest, cancellationToken);
        return Created(string.Empty, ApiResponse<SymbologyResponse>.Success(response));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<ICollection<SymbologyResponse>>>> GetSymbologiesAsync(CancellationToken cancellationToken = default)
    {
        var response = await _symbologyService.GetSymbologiesAsync(cancellationToken);
        return Ok(ApiResponse<ICollection<SymbologyResponse>>.Success(response));
            
    }

}
