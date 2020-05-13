using System;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.AspNetCore.Mvc;
using NsbRouterPlayground.Core.Domain;
using NsbRouterPlayground.Infrastructure.Persistence;
using NsbRouterPlayground.Integration.Messages.Events;
using NServiceBus;

namespace NsbRouterPlayground.WebApi.Features.Vendors {

   [ApiController]
   [Route("[controller]")]
   public class VendorsController : ControllerBase {

      private readonly SampleContext _ctx;
      private readonly IMessageSession _endpoint;

      public VendorsController(SampleContext ctx, IMessageSession endpoint) {

         _ctx = ctx;
         _endpoint = endpoint;
      }

      [HttpGet("{id:int}", Name = "GetVendorById")]
      public IActionResult Get(int id) {

         return Ok(new {Id = id});
      }

      [HttpPost("")]
      public async Task<IActionResult> Create() {

         var uid = Guid.NewGuid();
         Vendor vendor = default;

         using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() {
            IsolationLevel = IsolationLevel.RepeatableRead
         }, TransactionScopeAsyncFlowOption.Enabled)) {

            // Save vendor
            vendor = new Vendor() {
               Name = $"Sample vendor {uid}",
               UId = uid
            };

            _ctx.Vendors.Add(vendor);
            await _ctx.SaveChangesAsync();

            // Publish message
            await _endpoint.Publish(new VendorCreated() {
               Name = vendor.Name,
               Uid = vendor.UId
            });

            scope.Complete();
         }

         var location = new Uri(Url.Link("GetVendorById", new {
            vendor.Id
         }));

         return Created(location, vendor);
      }
   }
}