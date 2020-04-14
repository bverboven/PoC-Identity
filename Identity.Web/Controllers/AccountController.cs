using System.Linq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Web.Controllers
{
    public class AccountController : Controller
    {
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
