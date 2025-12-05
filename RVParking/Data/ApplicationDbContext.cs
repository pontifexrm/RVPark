using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace RVParking.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<ApplicationUser>? ApplicationUsers { get; set; }
        public DbSet<Bkg_Availability>? bkg_Availabilities { get; set; }
        public DbSet<Bkg_Booking>? bkg_Bookings { get; set; }
        public DbSet<Bkg_Payment>? bkg_Payments { get; set; }
        public DbSet<Bkg_Property>? bkg_Properties { get; set; }
        public DbSet<Bkg_User>? bkg_Users { get; set; }
        public DbSet<Bkg_Review>? bkg_Reviews { get; set; }
        public DbSet<Contact>? contacts { get; set; }
        public DbSet<AppLog>? AppLogs { get; set; }
        public async Task<Bkg_User?> GetBkgUserByIdAsync(int userId)
        {
            return bkg_Users != null ? await bkg_Users.FirstOrDefaultAsync(u => u.UserId == userId) : null;
        }
        public async Task<Bkg_User?> GetBkgUserByAppUserIdAsync(string appUserId)
        {
            return bkg_Users != null ? await bkg_Users.FirstOrDefaultAsync(u => u.AppUserId == appUserId): null;
        }

        public async Task<bool> BookingAnyAsync(int userId)
        {
            return await (bkg_Bookings?.AnyAsync(u => u.UserId == userId) ?? Task.FromResult(false));
        }
        public async Task<bool> ReviewAnyAsync(int userId)
        {
            return await (bkg_Reviews?.AnyAsync(r => r.UserId == userId) ?? Task.FromResult(false));
        }
        public async Task<Bkg_User?> DeleteBkgUserAsync(int usrId)
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var buser = await bkg_Users.FindAsync(usrId);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            if (buser == null)
            {
                return null;
            }

            bkg_Users.Remove(buser);
            await SaveChangesAsync();
            return buser;
        }

        public bool Bkg_AvailableAllSync(DateTime Fmdate, DateTime Todate)
        {
            if (bkg_Availabilities == null)
            {
                return false;
            }
            bool res = bkg_Availabilities
                .Where(b => b.DateAvailable >= Fmdate && b.DateAvailable <= Todate)
                .All(b => b.Available);
            return res;
            // TODO: this return true even where there are NO records in the table.

        }


        public async Task<List<string>> GetUserRolesAsync(string userId)
        {
            var roles = await (from userRole in UserRoles
                               join role in Roles on userRole.RoleId equals role.Id
                               where userRole.UserId == userId
                               select role.Name).ToListAsync();

            return roles;
        }
        public async Task RemoveUserRoleAsync(string userId, string roleId)
        {
            var userRole = await UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

            if (userRole != null)
            {
                UserRoles.Remove(userRole);
                await SaveChangesAsync();
            }
        }
        public async Task AddUserRoleAsync(string userId, string roleId)
        {
            var userRole = await UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

            if (userRole != null)
            {
                UserRoles.Remove(userRole);
                await SaveChangesAsync();
            }
        }
        public async Task<int> GetUserCountAsync()
        {
            return await (bkg_Users?.CountAsync() ?? Task.FromResult(0));
        }
        public async Task<int> GetIdentityUserCountAsync()
        {
            return await (ApplicationUsers?.CountAsync() ?? Task.FromResult(0));
        }
        public async Task<int> CountUserConfirmedAsync()
        {
            return await (ApplicationUsers?.CountAsync(u => u.EmailConfirmed) ?? Task.FromResult(0));
        }   
    }
}
