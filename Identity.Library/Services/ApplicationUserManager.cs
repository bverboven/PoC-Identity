using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Identity.Library.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Identity.Library.Services
{
    public interface IApplicationUserManager
    {
        Task<RefreshToken> CreateRefreshToken(ApplicationUser user, int expiresInHours = 24);
        Task<bool> VerifyRefreshToken(ApplicationUser user, string token);
        Task<ApplicationUser> FindByNameAsync(string userName);
        Task<bool> CheckPasswordAsync(ApplicationUser user, string password);
        Task<string> GetEmailAsync(ApplicationUser user);
    }

    public class ApplicationUserManager : UserManager<ApplicationUser>//, IApplicationUserManager
    {
        private readonly IRefreshTokenStore _refreshTokenStore;
        public ApplicationUserManager(IUserStore<ApplicationUser> store, IRefreshTokenStore refreshTokenStore, IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<ApplicationUser> passwordHasher, IEnumerable<IUserValidator<ApplicationUser>> userValidators, IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators, ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<ApplicationUser>> logger)
            : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
            _refreshTokenStore = refreshTokenStore;
        }


        public Task<RefreshToken> CreateRefreshToken(ApplicationUser user, int expiresInHours = 24)
        {
            return _refreshTokenStore.Create(user.Id, expiresInHours);
        }
        public Task<bool> VerifyRefreshToken(ApplicationUser user, string token)
        {
            return _refreshTokenStore.VerifyAndDelete(token, user.Id);
        }
    }
}
