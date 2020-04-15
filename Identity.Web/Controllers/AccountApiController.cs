using System;
using Identity.Library.Entities;
using Identity.Library.Helpers;
using Identity.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Identity.Library.Services;

namespace Identity.Web.Controllers
{
    // use Bearer scheme as default for api
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ApiController]
    [Route("api/auth")]
    public class AccountApiController : ControllerBase
    {
        static readonly string[] WANTED_CLAIMS = { ClaimTypes.NameIdentifier, ClaimTypes.Name, ClaimTypes.Role, "fullname", "permission" };

        private readonly ApplicationUserManager _userManager;
        private readonly IUserClaimsPrincipalFactory<ApplicationUser> _claimsFactory;
        private readonly ApplicationSignInManager _signInManager;
        private readonly JwtTokenHelper _tokenHelper;
        public AccountApiController(ApplicationUserManager userManager, IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory, ApplicationSignInManager signInManager)
        {
            _userManager = userManager;
            _claimsFactory = claimsFactory;
            _signInManager = signInManager;
            _tokenHelper = new JwtTokenHelper(new JwtTokenOptions { Key = "secret".PadRight(16, 'x') });
        }


        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody]ApiLoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);

            var loginResult = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (user != null && loginResult.Succeeded)
            {
                var tokenResult = await CreateToken(user);
                return Ok(tokenResult);
            }

            return Unauthorized();
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] ApiRefreshModel model)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null && await _userManager.VerifyRefreshToken(user, model.RefreshToken))
            {
                var tokenResult = await CreateToken(user);
                return Ok(tokenResult);
            }

            return Unauthorized();
        }
        private async Task<object> CreateToken(ApplicationUser user)
        {
            var principal = await _claimsFactory.CreateAsync(user);
            var claims = (await _userManager.GetClaimsAsync(user))
                .Concat(principal.Claims)
                .Where(c => WANTED_CLAIMS.Contains(c.Type))
                .ToList();

            var expires = DateTime.Now.AddMinutes(1);
            var token = _tokenHelper.Create(claims, expires);

            var refreshToken = await _userManager.CreateRefreshToken(user);

            return new
            {
                accessToken = new
                {
                    token,
                    expires
                },
                refreshToken = new
                {
                    refreshToken.Token,
                    refreshToken.Expires
                }
            };
        }


        // requires authorized token from scheme "Bearer"
        [HttpGet("claims")]
        public async Task<IActionResult> GetClaims()
        {
            var token = await HttpContext.GetTokenAsync("Bearer", "access_token");
            var tokenModel = new TokenModel(token);
            var userName = User.Identity.Name;
            var claims = User.Claims
                .Select(c => new { c.Type, c.Value })
                .ToList();
            var cookieValues = Request.Cookies.Select(c => new { c.Key, c.Value });
            var headerValues = Request.Headers.Select(h => new { h.Key, h.Value });

            return Ok(new
            {
                User.Identity.IsAuthenticated,
                userName,
                claims,
                token = tokenModel,
                cookieValues,
                headerValues
            });
        }


        // requires authorized bearer token and policy "IsAdmin"
        [Authorize("IsAdmin")]
        [HttpGet("is-admin")]
        public IActionResult CheckAdmin()
        {
            return Ok(new { isAdmin = true });
        }

        // requires authorized bearer token and policy "CanRead"
        [Authorize("CanRead")]
        [HttpGet("can-read")]
        public IActionResult CanRead()
        {
            return Ok(new { canRead = true });
        }

        // requires authorized bearer token and policy "CanDelete"
        [Authorize("CanDelete")]
        [HttpGet("can-delete")]
        public IActionResult CanDelete()
        {
            return Ok(new { canDelete = true });
        }
    }
}
