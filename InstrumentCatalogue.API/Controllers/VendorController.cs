using InstrumentCatalogue.Application.DTOs;
using InstrumentCatalogue.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading;

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
        

        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<VendorResponse?>> GetVendorByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var vendorResponse = await _vendorService.GetVendorByIdAsync(id, cancellationToken);

            if(vendorResponse == null)
                return NotFound();

            return Ok(vendorResponse);


        }

        [HttpGet]
        public async Task<ActionResult<ICollection<VendorResponse>>> GetVendorsAsync(CancellationToken cancellationToken = default)
        {
            var vendorResponseList = await _vendorService.GetVendorsAsync(cancellationToken);

            return Ok(vendorResponseList);


        }

        [HttpPatch]
        [Route("{id}")]
        public async Task<ActionResult<VendorResponse>> UpdateVendorAsync(int id, [FromBody] UpdateVendorRequest request, CancellationToken cancellationToken = default)
        {
           var vendorResponse = await _vendorService.UpdateVendorAsync(id, request, cancellationToken);

            if(vendorResponse == null)
                return NotFound();

            return Ok(vendorResponse);

        }


    }
}
