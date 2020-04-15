using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace Identity.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var seed = args.Contains("/seed");
            if (seed)
            {
                args = args.Except(new[] { "/seed" }).ToArray();
            }
            var cleanup = args.Contains("/clean");
            if (cleanup)
            {
                args = args.Except(new[] { "/clean" }).ToArray();
            }

            var host = CreateHostBuilder(args).Build();
            var logger = host.Services.GetRequiredService<ILogger<Program>>();

            if (seed)
            {
                logger.LogInformation("Seeding database...");
                var config = host.Services.GetRequiredService<IConfiguration>();
                var connectionString = config.GetConnectionString("IdentityContextConnection");
                SeedData.EnsureSeedData(connectionString);
                logger.LogInformation("Done seeding database.");
            }

            if (cleanup)
            {
                logger.LogInformation("Cleaning up database...");
                var config = host.Services.GetRequiredService<IConfiguration>();
                var connectionString = config.GetConnectionString("IdentityContextConnection");
                CleanData.CleanUp(connectionString);
                logger.LogInformation("Done cleaning database.");
            }

            logger.LogInformation("Starting host...");
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
