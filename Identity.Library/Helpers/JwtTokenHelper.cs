using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Identity.Library.Helpers
{
    public class JwtTokenHelper
    {
        private readonly string _key;
        private readonly string _algorithm;
        public JwtTokenHelper(JwtTokenOptions options)
        {
            _key = options.Key;
            _algorithm = options.Algorithm ?? SecurityAlgorithms.HmacSha256Signature;
        }


        public string Create(IEnumerable<Claim> claims, DateTime? expires = null)
        {
            var secretKey = Encoding.ASCII.GetBytes(_key);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                // only 1 minute (for testing expiration)
                Expires = expires ?? DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKey), _algorithm)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
