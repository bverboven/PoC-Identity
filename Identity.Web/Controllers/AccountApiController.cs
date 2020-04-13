using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Identity.Library.Entities;
using Identity.Library.Helpers;
using Identity.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Web.Controllers
{
    [ApiController]
    [Route("api")]
    public class AccountApiController : ControllerBase
    {
        static readonly string[] WANTED_CLAIMS = { ClaimTypes.NameIdentifier, ClaimTypes.Name, "fullname" };

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserClaimsPrincipalFactory<ApplicationUser> _claimsFactory;
        private readonly JwtTokenHelper _tokenHelper;
        public AccountApiController(UserManager<ApplicationUser> userManager, IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory)
        {
            _userManager = userManager;
            _claimsFactory = claimsFactory;
            _tokenHelper = new JwtTokenHelper(new JwtTokenOptions { Key = "secret".PadRight(16, 'x') });
        }


        [AllowAnonymous]
        [HttpGet]
        public IActionResult GetDataFromCookie()
        {
            var cookieValues = Request.Cookies.Select(c => new { c.Key, c.Value });
            var headerValues = Request.Headers.Select(h => new { h.Key, h.Value });
            var userName = User.Identity.Name;
            var claims = User.Claims.Select(c => new { c.Type, c.Value });
            var tokenClaims = User.Claims.Where(c => WANTED_CLAIMS.Contains(c.Type));
            var token = _tokenHelper.Create(tokenClaims);

            return Ok(new
            {
                User.Identity.IsAuthenticated,
                userName,
                claims,
                cookieValues,
                headerValues,
                token
            });
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody]ApiLoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var principal = await _claimsFactory.CreateAsync(user);
                var claims = (await _userManager.GetClaimsAsync(user))
                    .Concat(principal.Claims)
                    .Where(c => WANTED_CLAIMS.Contains(c.Type))
                    .ToList();
                var token = _tokenHelper.Create(claims);
                return Ok(new { token });
            }

            return Unauthorized();
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet("claims")]
        public IActionResult GetClaims()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value });
            return Ok(new
            {
                User.Identity.IsAuthenticated,
                claims
            });
        }
    }
}
