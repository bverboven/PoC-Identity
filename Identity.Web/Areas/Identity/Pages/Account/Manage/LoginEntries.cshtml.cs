using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Identity.Library.Entities;
using Identity.Library.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Internal;

namespace Identity.Web.Areas.Identity.Pages.Account.Manage
{
    public class LoginEntriesModel : PageModel
    {
        public class UserLoginEntryModel
        {
            [Display(Name = "Can read entries")]
            public bool CanRead { get; set; }
        }

        private readonly ApplicationUserManager _userManager;
        private readonly IUserClaimsPrincipalFactory<ApplicationUser> _userClaimsPrincipalFactory;
        private readonly ApplicationSignInManager _signInManager;
        public LoginEntriesModel(ApplicationUserManager userManager, IUserClaimsPrincipalFactory<ApplicationUser> userClaimsPrincipalFactory,
            ApplicationSignInManager signInManager)
        {
            _userManager = userManager;
            _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
            _signInManager = signInManager;
        }


        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public UserLoginEntryModel Input { get; set; }

        private async Task LoadAsync(ApplicationUser user)
        {
            var identity = await _userClaimsPrincipalFactory.CreateAsync(user);
            var claims = identity.Claims;

            Input = new UserLoginEntryModel
            {
                CanRead = claims.Any(c => c.Type == "permission" && c.Value == "can_read")
            };
        }
        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var identity = await _userClaimsPrincipalFactory.CreateAsync(user);
            var currentClaim = identity.Claims.FirstOrDefault(c => c.Type == "permission" && c.Value == "can_read");

            if (Input.CanRead != (currentClaim?.Value == "can_read"))
            {
                if (Input.CanRead)
                {
                    await _userManager.AddClaimAsync(user, new Claim("permission", "can_read"));
                }
                else
                {
                    await _userManager.RemoveClaimAsync(user, currentClaim);
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your permissions have been updated";
            return RedirectToPage();
        }
    }
}
