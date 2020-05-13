using System;
using System.Data.Entity;
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
      public async Task<IActionResult> Get(int id) {

         var vendor = await _ctx.Vendors.FindAsync(id);

         return vendor != null
            ? (IActionResult) Ok(vendor)
            : NotFound();
      }

      [HttpPost("")]
      public async Task<IActionResult> Create(bool failBeforePublish = false, bool failBeforeCommit = false) {

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

            if (failBeforePublish) {
               throw new Exception($"Thrown because of {nameof(failBeforePublish)}");
            }

            // Publish message
            await _endpoint.Publish(new VendorCreated() {
               Name = vendor.Name,
               Uid = vendor.UId
            });

            if (failBeforeCommit)
            {
               throw new Exception($"Thrown because of {nameof(failBeforeCommit)}");
            }

            scope.Complete();
         }

         var location = new Uri(Url.Link("GetVendorById", new {
            vendor.Id
         }));

         return Created(location, vendor);
      }
   }
}