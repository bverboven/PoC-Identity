using Identity.Library.Data;
using Identity.Library.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace Identity.Web
{
    public class SeedData
    {
        public static void EnsureSeedData(string connectionString)
        {
            var services = new ServiceCollection();
            services.AddLogging(o => o.AddConsole());
            services.AddDbContext<IdentityContext>(options =>
            {
                //options.UseSqlServer(connectionString);
                options.UseMySql(connectionString);
            });

            services
                .AddIdentity<ApplicationUser, IdentityRole>(o =>
                {
                    o.SignIn.RequireConfirmedAccount = true;

                    o.User.RequireUniqueEmail = true;

                    o.ClaimsIdentity.RoleClaimType = "role";

                    o.Password.RequireDigit = false;
                    o.Password.RequireLowercase = false;
                    o.Password.RequireUppercase = false;
                    o.Password.RequireNonAlphanumeric = false;
                    o.Password.RequiredUniqueChars = 1;
                    o.Password.RequiredLength = 4;
                })
                .AddEntityFrameworkStores<IdentityContext>()
                .AddDefaultTokenProviders();

            using var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();

            var logger = scope.ServiceProvider.GetRequiredService<ILogger<SeedData>>();

            var context = scope.ServiceProvider.GetService<IdentityContext>();
            context.Database.Migrate();

            var adminEmail = "admin@regira.com";
            var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var adminUser = userMgr.FindByNameAsync(adminEmail).Result;
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };
                var result = userMgr.CreateAsync(adminUser, "admin").Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }

                var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var adminRole = new IdentityRole("Administrator");
                roleMgr.CreateAsync(adminRole).Wait();
                foreach (var claim in new[] {
                    new Claim("permission", "can_read"),
                    new Claim("permission", "can_create"),
                    new Claim("permission", "can_update"),
                    new Claim("permission", "can_delete")
                })
                {
                    roleMgr.AddClaimAsync(adminRole, claim).Wait();
                }
                userMgr.AddToRoleAsync(adminUser, adminRole.Name).Wait();

                logger.LogInformation("admin created");
            }
            else
            {
                logger.LogInformation("admin already exists");
            }
        }
    }
}
