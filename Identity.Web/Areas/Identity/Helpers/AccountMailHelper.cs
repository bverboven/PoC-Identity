using Identity.Library.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Identity.Web.Areas.Identity.Helpers
{
    public class AccountMailHelper<TUser>
        where TUser : IdentityUser, new()
    {
        private readonly UserManager<TUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly LinkGenerator _linkGenerator;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public AccountMailHelper(UserManager<TUser> userManager, IEmailSender emailSender, LinkGenerator linkGenerator, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _linkGenerator = linkGenerator;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task SendVerificationEmail(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                throw new Exception($"Unable to load user with Email '{email}'.");
            }

            await SendVerificationEmail(user);
        }
        public async Task SendVerificationEmail(TUser user)
        {
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var context = _httpContextAccessor.HttpContext;
            var callbackUrl = _linkGenerator.GetUriByPage(
                context,
                "/Account/ConfirmEmail",
                null,
                new { area = "Identity", userId = user.Id, code },
                context.Request.Scheme
            );
            await _emailSender.SendEmailAsync(
                user.Email,
                "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>."
            );
        }

        public async Task SendChangeEmail(TUser user, string newEmail)
        {
            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateChangeEmailTokenAsync(user, newEmail);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var context = _httpContextAccessor.HttpContext;
            var callbackUrl = _linkGenerator.GetUriByPage(
                context,
                "/Account/ConfirmEmailChange",
                null,
                new { userId, code, email = newEmail },
                context.Request.Scheme
            );
            await _emailSender.SendEmailAsync(
                newEmail,
                "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>."
            );
        }
    }
}
