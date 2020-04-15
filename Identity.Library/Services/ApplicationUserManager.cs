using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Identity.Library.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Identity.Library.Services
{
    public class ApplicationUserManager : UserManager<ApplicationUser>//, IApplicationUserManager
    {
        private readonly IRefreshTokenStore _refreshTokenStore;
        private readonly ILoginEntryStore _loginEntryStore;
        public ApplicationUserManager(IUserStore<ApplicationUser> store, IRefreshTokenStore refreshTokenStore, ILoginEntryStore loginEntryStore,
            IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<ApplicationUser> passwordHasher,
            IEnumerable<IUserValidator<ApplicationUser>> userValidators, IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators,
            ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<ApplicationUser>> logger)
            : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
            _refreshTokenStore = refreshTokenStore;
            _loginEntryStore = loginEntryStore;
        }


        // RefreshToken
        public Task<RefreshToken> CreateRefreshToken(ApplicationUser user, int expiresInHours = 24)
        {
            return _refreshTokenStore.Create(user.Id, expiresInHours);
        }
        public Task<bool> VerifyRefreshToken(ApplicationUser user, string token)
        {
            return _refreshTokenStore.VerifyAndDelete(token, user.Id);
        }

        // LoginEntry
        public async Task<IEnumerable<LoginEntry>> GetLoginEntries(ApplicationUser user)
        {
            return await _loginEntryStore.List(user.Id);
        }
        public async Task RemoveLoginEntry(ApplicationUser user, LoginEntry item)
        {
            item.UserId = user.Id;
            await _loginEntryStore.Delete(item);
        }
    }
}
