using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace Identity.Library.Helpers
{
    public class JwtTokenHelper
    {
        private readonly JwtSecurityTokenHandler _tokenHandler;
        private readonly string _key;
        private readonly string _algorithm;
        public JwtTokenHelper(string key, string algorithm = SecurityAlgorithms.HmacSha256Signature)
        {
            _tokenHandler = new JwtSecurityTokenHandler();
            _key = key;
            _algorithm = algorithm;
        }


        public string Create(string userId, DateTime? expires = null, params Claim[] claims)
        {
            var secretKey = Encoding.ASCII.GetBytes(_key);
            if (!claims.Any())
            {
                claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId) };
            }
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expires ?? DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKey), _algorithm)
            };
            var token = _tokenHandler.CreateToken(tokenDescriptor);
            return _tokenHandler.WriteToken(token);
        }
    }
}
