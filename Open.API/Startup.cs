
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Open.API.Common.Extensions;
using Open.API.Core.GraphQL.Extensions;
using Open.API.Domain;
using Open.API.Extensions;
using Open.API.Infrastructure;

namespace Open.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            string provider = Configuration["Provider"];
            string connectionString = Configuration.GetSection("ConnectionStrings").GetSection(provider)["Default"];
            string migrationAssembly = $"Open.API.Migrations.{provider}";

            services.AddDbContext<IUnitOfWork, UnitOfWork>(options => options.UseProvider(provider, connectionString, migrationAssembly));

            services.AddControllers();



            services.RegisterMediatR();
            services.RegisterGraphQL();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.EnableGraphQL();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
