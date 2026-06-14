using InstrumentCatalogue.Application.DTOs;
using InstrumentCatalogue.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace InstrumentCatalogue.API.Controllers
{
    [ApiController]
    [Route("api/v1/vendors")]
    public class VendorController : ControllerBase
    {
        private readonly IVendorService _vendorService;
        public VendorController(IVendorService vendorService)
        {
            _vendorService = vendorService ?? throw new ArgumentNullException(nameof(vendorService));
        }

        [HttpPost]
        public async Task<ActionResult<VendorResponse>> CreateVendorAsync([FromBody] CreateVendorRequest vendorRequest, CancellationToken cancellationToken =default)
        {
            var vendorResponse = await _vendorService.CreateVendorAsync(vendorRequest, cancellationToken);

            return Created(string.Empty, vendorResponse);
            
        }
    }
}
