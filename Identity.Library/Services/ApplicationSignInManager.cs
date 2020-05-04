using Identity.Library.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace Identity.Library.Services
{
    public class ApplicationSignInManager : SignInManager<ApplicationUser>
    {
        private readonly ILoginEntryStore _loginEntryStore;
        public ApplicationSignInManager(UserManager<ApplicationUser> userManager, ILoginEntryStore loginEntryStore, IHttpContextAccessor contextAccessor, IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory, IOptions<IdentityOptions> optionsAccessor, ILogger<SignInManager<ApplicationUser>> logger, IAuthenticationSchemeProvider schemes, IUserConfirmation<ApplicationUser> confirmation)
            : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
        {
            _loginEntryStore = loginEntryStore;
        }


        public override async Task<SignInResult> CheckPasswordSignInAsync(ApplicationUser user, string password, bool lockoutOnFailure)
        {
            var result = await base.CheckPasswordSignInAsync(user, password, lockoutOnFailure);
            await LogEntry(user, result.ToString());

            return result;
        }
        public override async Task<SignInResult> ExternalLoginSignInAsync(string loginProvider, string providerKey, bool isPersistent, bool bypassTwoFactor)
        {
            var result = await base.ExternalLoginSignInAsync(loginProvider, providerKey, isPersistent, bypassTwoFactor);
            var user = await UserManager.FindByLoginAsync(loginProvider, providerKey);
            await LogEntry(user, result.ToString());

            return result;
        }

        private async Task LogEntry(ApplicationUser user, string status)
        {
            // add entry for each login attempt
            var loginEntry = new LoginEntry
            {
                UserId = user.Id,
                IPAddress = Context.Connection.RemoteIpAddress,
                Status = status
            };
            await _loginEntryStore.Save(loginEntry);
        }
    }
}
