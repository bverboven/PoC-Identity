using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Identity.Library.Entities;
using Identity.Library.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationUserManager _userManager;
        public AccountController(ApplicationUserManager userManager)
        {
            _userManager = userManager;
        }


        public IActionResult Claims()
        {
            return View();
        }
        [AllowAnonymous]
        public IActionResult UserData()
        {
            var cookieValues = Request.Cookies.Select(c => new { c.Key, c.Value });
            var headerValues = Request.Headers.Select(h => new { h.Key, h.Value });
            var userName = User.Identity.Name;
            var claims = User.Claims.Select(c => new { c.Type, c.Value });

            return Json(new
            {
                User.Identity.IsAuthenticated,
                userName,
                claims,
                cookieValues,
                headerValues
            });
        }

        [Authorize("CanRead")]
        public async Task<IActionResult> LoginEntries()
        {
            var userId = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByIdAsync(userId);
            var entries = await _userManager.GetLoginEntries(user);
            return View(entries.ToList());
        }
        [HttpPost]
        [Authorize("CanDelete")]
        public async Task<IActionResult> DeleteLoginEntry(long id)
        {
            var userId = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByIdAsync(userId);
            await _userManager.RemoveLoginEntry(user, new LoginEntry { Id = id });
            return RedirectToAction("LoginEntries");
        }

        [AllowAnonymous]
        public IActionResult Login(string returnUrl = "/")
        {
            return Challenge(new AuthenticationProperties { RedirectUri = returnUrl });
        }
        public IActionResult Logout(string returnUrl = "/")
        {
            return SignOut(new AuthenticationProperties { RedirectUri = returnUrl });
        }
    }
}
