using Microsoft.AspNetCore.Identity;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Identity.Library.Helpers
{
    public class ExternalAccountHelper<TUser>
        where TUser : IdentityUser, new()
    {
        private readonly UserManager<TUser> _userManager;
        private readonly SignInManager<TUser> _signInManager;
        private readonly AccountMailHelper<TUser> _mailHelper;
        public ExternalAccountHelper(UserManager<TUser> userManager, SignInManager<TUser> signInManager, AccountMailHelper<TUser> mailHelper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _mailHelper = mailHelper;
        }

        public async Task<IdentityResult> CreateUser(ExternalLoginInfo info, bool signInWhenConfirmationNotRequired = true)
        {
            var principal = info.Principal;
            var username = principal.FindFirstValue(ClaimTypes.Email);

            var user = new TUser
            {
                UserName = username,
                Email = username
            };

            var result = await _userManager.CreateAsync(user);
            if (result.Succeeded)
            {
                result = await _userManager.AddLoginAsync(user, info);
                if (result.Succeeded)
                {
                    // send confirmation mail
                    await _mailHelper.SendVerificationEmail(user);

                    // If account confirmation is required, we need to show the link if we don't have a real email sender
                    if (signInWhenConfirmationNotRequired && !_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        await _signInManager.SignInAsync(user, false, info.LoginProvider);
                    }
                }
            }

            return result;
        }

        public async Task<IdentityResult> UpdateUser(ExternalLoginInfo info, Action<TUser> configureUser = null)
        {
            var username = info.Principal.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByNameAsync(username);

            configureUser?.Invoke(user);

            return await _userManager.UpdateAsync(user);
        }
    }
}
