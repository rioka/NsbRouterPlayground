using System.Data.Entity.ModelConfiguration;
using NsbRouterPlayground.Core.Domain;

namespace NsbRouterPlayground.Infrastructure.Persistence.Configurations {

   internal class VendorConfiguration : EntityTypeConfiguration<Vendor> {

      public VendorConfiguration() {

         Property(v => v.Name)
            .IsRequired()
            .HasMaxLength(100);
      }
   }
}