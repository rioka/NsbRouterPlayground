using System.Data.Entity;
using System.Data.SqlClient;
using System.Reflection;
using NsbRouterPlayground.Core.Domain;

namespace NsbRouterPlayground.Infrastructure.Persistence {

   public class SampleContext : DbContext {

      public SampleContext(SqlConnection cn) : base(cn, false) {
      }

      public DbSet<Vendor> Vendors { get; set; }

      protected override void OnModelCreating(DbModelBuilder modelBuilder) {

         modelBuilder.Configurations.AddFromAssembly(Assembly.GetExecutingAssembly());
      }
   }
}
