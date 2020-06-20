
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Open.API
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostinContext, config) =>
                {
                    var env = hostinContext.HostingEnvironment;

                    config.AddJsonFile("appsettings.json", false, reloadOnChange: true)
                          .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, reloadOnChange: true);

                    if (env.IsDevelopment())
                    {
                        config.AddUserSecrets("08e9423d-ed25-449f-aa6e-a8506d5d826e", reloadOnChange: true);
                    }
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
