using Identity.Library.Data;
using Identity.Library.Entities;
using Identity.Library.Helpers;
using Identity.Library.Services;
using Identity.Web.Mail;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

[assembly: HostingStartup(typeof(Identity.Web.Areas.Identity.IdentityHostingStartup))]
namespace Identity.Web.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
                var identityConfig = context.Configuration.GetSection("Identity");

                services
                    // Mail (SendGrid)
                    .Configure<MailOptions>(identityConfig.GetSection("SendGrid"))
                    .AddScoped(p => p.GetRequiredService<IOptionsSnapshot<MailOptions>>().Value)
                    .AddTransient<IEmailSender, IdentityEmailSender>();

                services
                    // DbContext
                    .AddDbContext<IdentityContext>(options =>
                    {
                        options.UseSqlServer(
                            context.Configuration.GetConnectionString("IdentityContextConnection"),
                            // since the DbContext is in another project, include the assemblyname for migrations
                            b => b.MigrationsAssembly("Identity.Web")
                        );
                    })
                    .AddTransient<IRefreshTokenStore, RefreshTokenStore>()
                    // Identity
                    .AddIdentity<ApplicationUser, IdentityRole>(o =>
                    {
                        o.SignIn.RequireConfirmedAccount = true;

                        o.User.RequireUniqueEmail = true;

                        o.Password.RequireDigit = false;
                        o.Password.RequireLowercase = false;
                        o.Password.RequireUppercase = false;
                        o.Password.RequireNonAlphanumeric = false;
                        o.Password.RequiredUniqueChars = 1;
                        o.Password.RequiredLength = 4;
                    })
                    .AddEntityFrameworkStores<IdentityContext>()
                    .AddUserManager<ApplicationUserManager>()
                    .AddClaimsPrincipalFactory<ApplicationUserClaimsFactory>()
                    .AddDefaultTokenProviders()
                    .AddDefaultUI();

                services
                    // requires HttpContextAccessor
                    .AddTransient<AccountMailHelper<ApplicationUser>>();

                // external logins
                services
                    // get AuthenticationBuilder
                    .AddAuthentication()
                    // Google
                    .AddGoogle(o =>
                    {
                        o.ClientId = identityConfig["Google:ClientId"];
                        o.ClientSecret = identityConfig["Google:ClientSecret"];
                    })
                    // Microsoft
                    .AddMicrosoftAccount(o =>
                    {
                        o.ClientId = identityConfig["Microsoft:ClientId"];
                        o.ClientSecret = identityConfig["Microsoft:ClientSecret"];
                    })
                    // Facebook
                    .AddFacebook(o =>
                    {
                        o.AppId = identityConfig["Facebook:ClientId"];
                        o.AppSecret = identityConfig["Facebook:ClientSecret"];
                    })
                    // Twitter (doesn't seem to work with localhost)
                    .AddTwitter(o =>
                    {
                        o.ConsumerKey = identityConfig["Google:ClientId"];
                        o.ConsumerSecret = identityConfig["Google:ClientSecret"];
                        o.RetrieveUserDetails = true;
                    });
            });
        }
    }
}