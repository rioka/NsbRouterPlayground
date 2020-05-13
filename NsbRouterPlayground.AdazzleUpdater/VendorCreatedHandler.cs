using System;
using System.Threading.Tasks;
using NsbRouterPlayground.Integration.Messages.Events;
using NServiceBus;

namespace NsbRouterPlayground.AdazzleUpdater {
   
   internal class VendorCreatedHandler : IHandleMessages<VendorCreated> {
   
      public async Task Handle(VendorCreated message, IMessageHandlerContext context) {

         Console.WriteLine($"Processing {message.GetType().Name} for client {message.Uid}... ");
         await Task.Delay(DateTime.UtcNow.Millisecond);

         Console.WriteLine($"{message.GetType().Name} for client {message.Uid} processed");
      }
   }
}