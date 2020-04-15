using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Identity.Library.Data;
using Identity.Library.Entities;
using Microsoft.EntityFrameworkCore;

namespace Identity.Library.Services
{
    public interface ILoginEntryStore
    {
        Task<IEnumerable<LoginEntry>> List(string userId);
        Task Save(LoginEntry item);
        Task Delete(LoginEntry item);
        Task DeleteAll(string userId);
    }

    public class LoginEntryStore : ILoginEntryStore
    {
        public DbSet<LoginEntry> Items => DbContext.LoginEntries;

        protected readonly IdentityContext DbContext;
        public LoginEntryStore(IdentityContext dbContext)
        {
            DbContext = dbContext;
        }


        public async Task<IEnumerable<LoginEntry>> List(string userId)
        {
            return await Items
                .Where(x => x.UserId == userId)
                .ToListAsync();
        }

        public async Task Save(LoginEntry item)
        {
            if (item.Id <= 0)
            {
                Items.Add(item);
            }
            else
            {
                Items.Update(item);
            }

            await DbContext.SaveChangesAsync();
        }
        public async Task Delete(LoginEntry item)
        {
            Items.Remove(item);
            await DbContext.SaveChangesAsync();
        }
        public async Task DeleteAll(string userId)
        {
            var items = await List(userId);
            Items.RemoveRange(items);

            await DbContext.SaveChangesAsync();
        }
    }
}
