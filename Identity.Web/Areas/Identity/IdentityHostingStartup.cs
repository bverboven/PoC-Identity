﻿using Identity.Library.Data;
using Identity.Library.Entities;
using Identity.Library.Services;
using Identity.Web.Areas.Identity.Helpers;
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
                    // SendGrid
                    .Configure<MailOptions>(identityConfig.GetSection("SendGrid"))
                    .AddScoped(p => p.GetRequiredService<IOptionsSnapshot<MailOptions>>().Value)
                    .AddTransient<IEmailSender, IdentityEmailSender>();

                services
                    // DbContext
                    .AddDbContext<IdentityContext>(options =>
                    {
                        options.UseSqlServer(
                            context.Configuration.GetConnectionString("IdentityContextConnection"),
                            b =>
                            b.MigrationsAssembly("Identity.Web")
                        );
                    })
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
                    .AddDefaultUI()
                    .AddEntityFrameworkStores<IdentityContext>()
                    .AddDefaultTokenProviders()
                    .AddUserManager<ApplicationUserManager>()
                    .AddClaimsPrincipalFactory<ApplicationUserClaimsFactory>();

                services
                    // requires HttpContextAccessor
                    .AddTransient<AccountMailHelper<ApplicationUser>>();

                // external logins
                services
                    .AddAuthentication()
                    .AddGoogle(o =>
                    {
                        o.ClientId = identityConfig["Google:ClientId"];
                        o.ClientSecret = identityConfig["Google:ClientSecret"];
                    })
                    .AddMicrosoftAccount(o =>
                    {
                        o.ClientId = identityConfig["Microsoft:ClientId"];
                        o.ClientSecret = identityConfig["Microsoft:ClientSecret"];
                    })
                    .AddFacebook(o =>
                    {
                        o.AppId = identityConfig["Facebook:ClientId"];
                        o.AppSecret = identityConfig["Facebook:ClientSecret"];
                    })
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