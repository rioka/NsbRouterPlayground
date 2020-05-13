using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NsbRouterPlayground.Integration.Messages.Events;
using NServiceBus;

namespace NsbRouterPlayground.WebApi.Features.Vendors {

   [ApiController]
   [Route("[controller]")]
   public class VendorsController : ControllerBase {
      
      private readonly IMessageSession _endpoint;

      public VendorsController(IMessageSession endpoint) {

         _endpoint = endpoint;
      }

      [HttpGet("{id:int}", Name = "GetVendorById")]
      public IActionResult Get(int id) {

         return Ok(new { Id = id });
      }

      [HttpPost("")]
      public async Task<IActionResult> Create() {
         
         var vendor = new {Id = 1, Name = "Sample"};
         var location = new Uri(Url.Link("GetVendorById", new { vendor.Id }));

         await _endpoint.Publish(new VendorCreated() {
            Name = vendor.Name,
            Uid = Guid.NewGuid()
         });

         return Created(location, vendor);
      }
   }
}
