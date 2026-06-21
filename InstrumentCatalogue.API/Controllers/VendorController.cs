using FluentValidation;
using InstrumentCatalogue.API.ReadModels;
using InstrumentCatalogue.Application.DTOs.Vendor;
using InstrumentCatalogue.Application.DTOs.VendorInterface;
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
        public async Task<ActionResult<ApiResponse<VendorResponse>>> CreateVendorAsync([FromBody] CreateVendorRequest vendorRequest, IValidator<CreateVendorRequest> validator, CancellationToken cancellationToken =default)
        {
            await validator.ValidateAndThrowAsync(vendorRequest, cancellationToken);

            var vendorResponse = await _vendorService.CreateVendorAsync(vendorRequest, cancellationToken);

            return Created(string.Empty, ApiResponse<VendorResponse>.Success(vendorResponse));
            
        }
        

        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<ApiResponse<VendorResponse?>>> GetVendorByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var vendorResponse = await _vendorService.GetVendorByIdAsync(id, cancellationToken);

            return Ok(ApiResponse<VendorResponse?>.Success(vendorResponse));


        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<ICollection<VendorResponse>>>> GetVendorsAsync(CancellationToken cancellationToken = default)
        {
            var vendorResponseList = await _vendorService.GetVendorsAsync(cancellationToken);

            return Ok(ApiResponse<ICollection<VendorResponse>>.Success(vendorResponseList));


        }

        [HttpPatch]
        [Route("{id}")]
        public async Task<ActionResult<ApiResponse<VendorResponse?>>> UpdateVendorAsync(int id, [FromBody] UpdateVendorRequest request, IValidator<UpdateVendorRequest> validator, CancellationToken cancellationToken = default)
        {
           
            await validator.ValidateAndThrowAsync(request, cancellationToken);
            var vendorResponse = await _vendorService.UpdateVendorAsync(id, request, cancellationToken);


            return Ok(ApiResponse<VendorResponse?>.Success(vendorResponse));

        }

        [HttpPost]
        [Route("{vendorId}/interfaces")]
        public async Task<ActionResult<ApiResponse<VendorInterfaceResponse>>> CreateVendorInterfaceAsync(int vendorId, [FromBody] CreateVendorInterfaceRequest request, IValidator<CreateVendorInterfaceRequest> validator, CancellationToken cancellationToken = default )
        {
            await validator.ValidateAndThrowAsync(request, cancellationToken);

            var vendorInterfaceResponse = await _vendorService.CreateVendorInterfaceAsync(vendorId, request, cancellationToken);
            return Created(string.Empty, ApiResponse<VendorInterfaceResponse>.Success(vendorInterfaceResponse));
        }

        [HttpGet]
        [Route("{vendorId}/interfaces")]
        public async Task<ActionResult<ApiResponse<ICollection<VendorInterfaceResponse>>>> GetVendorInterfacesAsync(int vendorId, CancellationToken cancellationToken = default)
        {
          var vendorInterfaces = await _vendorService.GetVendorInterfacesAsync(vendorId, cancellationToken);
           return Ok(ApiResponse<ICollection<VendorInterfaceResponse>>.Success(vendorInterfaces));
        }

        [HttpGet]
        [Route("{vendorId}/interfaces/{id}")]
        public async Task<ActionResult<ApiResponse<VendorInterfaceResponse?>>> GetVendorInterfaceByIdAsync(int vendorId, int id, CancellationToken cancellationToken = default)
        {
           var vendorInterfaceResponse = await _vendorService.GetVendorInterfaceByIdAsync(vendorId, id, cancellationToken);

           return Ok(ApiResponse<VendorInterfaceResponse?>.Success(vendorInterfaceResponse));
        }

        [HttpPatch]
        [Route("{vendorId}/interfaces/{id}")]
        public async Task<ActionResult<ApiResponse<VendorInterfaceResponse?>>> UpdateVendorInterfaceAsync(int vendorId, int id, [FromBody]UpdateVendorInterfaceRequest request, IValidator<UpdateVendorInterfaceRequest>validator, CancellationToken cancellationToken = default)
        {
            await validator.ValidateAndThrowAsync(request, cancellationToken);

            var vendorInterface = await _vendorService.UpdateVendorInterfaceAsync(vendorId, id, request, cancellationToken);

            return Ok(ApiResponse<VendorInterfaceResponse?>.Success(vendorInterface));
        }



    }
}
