using System;

namespace NsbRouterPlayground.Core.Domain {

   public class Vendor {

      public int Id { get; private set; }

      public string Name { get; set; }

      public Guid UId { get; set; }
   }
}