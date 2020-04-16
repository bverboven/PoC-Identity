using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace Identity.Web
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddControllersWithViews()
                // use NewtonSoft Json for better serializing results
                .AddNewtonsoftJson();
            services.AddRazorPages();

            services.AddHttpContextAccessor();
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins",
                    corsBuilder =>
                    {
                        corsBuilder
                            .AllowAnyOrigin()
                            .SetIsOriginAllowedToAllowWildcardSubdomains()
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    });
            });

            // authentication
            services
                // use the same scheme than Identity framework
                .AddAuthentication(IdentityConstants.ApplicationScheme)
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
                // more info: https://docs.microsoft.com/en-us/aspnet/core/security/authorization/limitingidentitybyscheme?view=aspnetcore-3.1
                .AddJwtBearer(o =>
                {
                    o.RequireHttpsMetadata = false;
                    o.SaveToken = true;
                    o.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("secret".PadRight(16, 'x'))),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        // enable expiration check
                        ValidateLifetime = true,
                        // set on zero for testing (otherwise minimum expiration is around 5 min)
                        ClockSkew = TimeSpan.Zero
                    };
                });

            // authorization
            services
                .AddAuthorization(o =>
                {
                    o.AddPolicy("IsAdmin", c =>
                    {
                        c.RequireAuthenticatedUser();
                        c.RequireRole("Administrator");
                    });
                    o.AddPolicy("CanRead", c =>
                    {
                        c.RequireClaim("permission", "can_read");
                    });
                    o.AddPolicy("CanDelete", c =>
                    {
                        c.RequireClaim("permission", "can_delete");
                    });
                    o.AddPolicy("HasName", c =>
                    {
                        c.RequireClaim("name");
                    });
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // CORS
            app.UseCors("AllowAllOrigins");

            // default files
            var defaultFileOptions = new DefaultFilesOptions();
            defaultFileOptions.DefaultFileNames.Clear();
            defaultFileOptions.DefaultFileNames.Add("index.html");
            app.UseDefaultFiles(defaultFileOptions);
            // static files
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "dist")),
                //RequestPath = "/dist",
                DefaultContentType = "text/html"
            });

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints
                    .MapDefaultControllerRoute()
                    .RequireAuthorization();
                endpoints.MapRazorPages();
            });
        }
    }
}
