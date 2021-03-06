using System.Data.SqlClient;
using System.Reflection;
using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NsbRouterPlayground.Infrastructure.Persistence;

namespace NsbRouterPlayground.WebApi {

   public class Startup {
   
      public IConfiguration Configuration { get; }

      public Startup(IConfiguration configuration) {

         Configuration = configuration;
      }

      // This method gets called by the runtime. Use this method to add services to the container.
      // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
      public void ConfigureServices(IServiceCollection services) {

         services.AddSingleton<IConfiguration>(Configuration);
         services.AddScoped<SampleContext>();
         services.AddScoped(cn => new SqlConnection(Configuration.GetConnectionString("WebApi")));
         services.AddControllers();
      }

      public void ConfigureContainer(ContainerBuilder builder) {

         builder.RegisterAssemblyModules(Assembly.GetExecutingAssembly());
      }

      // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
      public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
      
         if (env.IsDevelopment()) {
            app.UseDeveloperExceptionPage();
         }

         app.UseRouting();

         app.UseEndpoints(endpoints => {
            endpoints.MapControllers();
         });
      }
   }
}