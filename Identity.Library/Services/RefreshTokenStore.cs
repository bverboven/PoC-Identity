using System;
using System.Security.Cryptography;
using Identity.Library.Data;
using Identity.Library.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Identity.Library.Services
{
    public interface IRefreshTokenStore
    {
        Task<RefreshToken> Create(string userId, int expiresInHours = 24);
        Task<bool> VerifyAndDelete(string token, string userId);
    }
    public class RefreshTokenStore : IRefreshTokenStore
    {
        public DbSet<RefreshToken> Items => DbContext.RefreshTokens;

        protected readonly IdentityContext DbContext;
        public RefreshTokenStore(IdentityContext dbContext)
        {
            DbContext = dbContext;
        }


        public async Task<RefreshToken> Create(string userId, int expiresInHours = 24)
        {
            var token = new RefreshToken
            {
                Token = GenerateRefreshToken(),
                UserId = userId,
                Expires = DateTime.Now.AddHours(expiresInHours)
            };
            Items.Add(token);
            await DbContext.SaveChangesAsync();
            return token;
        }
        public async Task<bool> VerifyAndDelete(string token, string userId)
        {
            var item = await Items.FindAsync(token);
            if (item != null && item.UserId == userId)
            {
                Items.Remove(item);
                await DbContext.SaveChangesAsync();
                return item.Expires >= DateTime.Now;
            }

            return false;
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
    }
}
