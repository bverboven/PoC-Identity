using Identity.Library.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using Identity.Library.Helpers;
using Identity.Library.Services;

namespace Identity.Web.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private readonly ApplicationSignInManager _signInManager;
        private readonly ApplicationUserManager _userManager;
        private readonly ILogger<ExternalLoginModel> _logger;
        private readonly AccountMailHelper<ApplicationUser> _accountMailHelper;
        private readonly ExternalAccountHelper<ApplicationUser> _externalAccountHelper;
        public ExternalLoginModel(
            ApplicationSignInManager signInManager,
            ApplicationUserManager userManager,
            ILogger<ExternalLoginModel> logger,
            AccountMailHelper<ApplicationUser> accountMailHelper,
            ExternalAccountHelper<ApplicationUser> externalAccountHelper)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _accountMailHelper = accountMailHelper;
            _externalAccountHelper = externalAccountHelper;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string LoginProvider { get; set; }

        public string ReturnUrl { get; set; }
        public bool ShowVerifyEmailLink { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }
            [Phone]
            [DataType(DataType.PhoneNumber)]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }
            [MaxLength(256)]
            [Display(Name = "Given name")]
            public string GivenName { get; set; }
            [MaxLength(256)]
            [Display(Name = "Family name")]
            public string FamilyName { get; set; }
        }

        public IActionResult OnGetAsync()
        {
            return RedirectToPage("./Login");
        }

        public IActionResult OnPost(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Page("./ExternalLogin", "Callback", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
        {
            returnUrl ??= Url.Content("~/");
            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false, true);
            if (result.Succeeded)
            {
                _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name, info.LoginProvider);
                return LocalRedirect(returnUrl);
            }
            if (result.IsLockedOut)
            {
                return RedirectToPage("./Lockout");
            }

            // If the user does not have an account, then create an account automatically.
            ReturnUrl = returnUrl;
            LoginProvider = info.LoginProvider;
            if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
            {
                var createResult = await _externalAccountHelper.CreateUser(info);
                if (createResult.Succeeded)
                {
                    var principal = info.Principal;
                    Input = new InputModel
                    {
                        Email = principal.FindFirstValue(ClaimTypes.Email),
                        PhoneNumber = principal.FindFirstValue(ClaimTypes.MobilePhone)
                                      ?? principal.FindFirstValue(ClaimTypes.HomePhone)
                                      ?? principal.FindFirstValue(ClaimTypes.OtherPhone),
                        GivenName = principal.FindFirstValue(ClaimTypes.GivenName),
                        FamilyName = principal.FindFirstValue(ClaimTypes.Surname)
                    };
                }
                else
                {
                    foreach (var error in createResult.Errors)
                    {
                        if (error.Code == "DuplicateUserName")
                        {
                            ShowVerifyEmailLink = true;
                            // skip to avoid error message for username AND email
                            continue;
                        }
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            return Page();
        }
        public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information during confirmation.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            if (ModelState.IsValid)
            {
                var result = await _externalAccountHelper.UpdateUser(info, u =>
                {
                    u.PhoneNumber = Input.PhoneNumber;
                    u.GivenName = Input.GivenName;
                    u.FamilyName = Input.FamilyName;
                });

                if (result.Succeeded)
                {
                    //If account confirmation is required, we need to show the link if we don't have a real email sender
                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("./RegisterConfirmation", new { Input.Email });
                    }

                    return LocalRedirect(returnUrl);
                }
            }

            LoginProvider = info.LoginProvider;
            ReturnUrl = returnUrl;
            return Page();
        }
        public async Task<IActionResult> OnPostSendVerificationEmailAsync()
        {
            await _accountMailHelper.SendVerificationEmail(Input.Email);

            if (_userManager.Options.SignIn.RequireConfirmedAccount)
            {
                return RedirectToPage("RegisterConfirmation", new { email = Input.Email });
            }

            return RedirectToPage();
        }
    }
}
