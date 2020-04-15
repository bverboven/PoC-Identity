using Identity.Library.Data;
using Identity.Library.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Identity.Web
{
    public class CleanData
    {
        public static void CleanUp(string connectionString)
        {
            var services = new ServiceCollection();
            services.AddLogging(o => o.AddConsole());
            services.AddDbContext<IdentityContext>(options => options.UseSqlServer(connectionString));

            services
                .AddTransient<IRefreshTokenStore, RefreshTokenStore>()
                .AddTransient<ILoginEntryStore, LoginEntryStore>();

            using var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();

            var logger = scope.ServiceProvider.GetRequiredService<ILogger<CleanData>>();
            var context = scope.ServiceProvider.GetService<IdentityContext>();

            var loginEntries = context.LoginEntries;
            loginEntries.RemoveRange(loginEntries);

            var refreshTokens = context.RefreshTokens;
            refreshTokens.RemoveRange(refreshTokens);

            context.SaveChanges();
        }
    }
}
