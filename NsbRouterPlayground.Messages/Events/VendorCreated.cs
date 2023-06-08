using System;

namespace NsbRouterPlayground.Integration.Messages.Events {

   public class VendorCreated {

      public Guid Uid { get; set; }

      public string Name { get; set; }
   }
}