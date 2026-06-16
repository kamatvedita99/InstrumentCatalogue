using InstrumentCatalogue.Application.DTOs;
using InstrumentCatalogue.Application.Services;
using Microsoft.AspNetCore.Http.HttpResults;
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

        [HttpPost]
        [Route("{vendorId}/interfaces")]
        public async Task<ActionResult<VendorInterfaceResponse>> CreateVendorInterfaceAsync(int vendorId, [FromBody] CreateVendorInterfaceRequest request, CancellationToken cancellationToken = default )
        {
            var vendorInterfaceResponse = await _vendorService.CreateVendorInterfaceAsync(vendorId, request, cancellationToken);
            return Created(string.Empty, vendorInterfaceResponse);
        }

        [HttpGet]
        [Route("{vendorId}/interfaces")]
        public async Task<ActionResult<ICollection<VendorInterfaceResponse>>> GetVendorInterfacesAsync(int vendorId, CancellationToken cancellationToken = default)
        {
          var vendorInterfaces = await _vendorService.GetVendorInterfacesAsync(vendorId, cancellationToken);
           return Ok(vendorInterfaces);
        }

        [HttpGet]
        [Route("interfaces/{id}")]
        public async Task<ActionResult<VendorInterfaceResponse?>> GetVendorInterfaceByIdAsync(int id, CancellationToken cancellationToken = default)
        {
           var vendorInterfaceResponse = await _vendorService.GetVendorInterfaceByIdAsync(id, cancellationToken);

            if( vendorInterfaceResponse == null) return NotFound();

            return Ok(vendorInterfaceResponse);
        }



    }
}
