using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace RVPark.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; } = null!;
        public DbSet<Bkg_Availability> bkg_Availabilities { get; set; } = null!;
        public DbSet<Bkg_Booking> bkg_Bookings { get; set; } = null!;
        public DbSet<Bkg_Payment> bkg_Payments { get; set; } = null!;
        public DbSet<Bkg_Property> bkg_Properties { get; set; } = null!;
        public DbSet<Bkg_User> bkg_Users { get; set; } = null!;
        public DbSet<Bkg_Review> bkg_Reviews { get; set; } = null!;
        public DbSet<Contact> contacts { get; set; } = null!;
        public DbSet<AppLog> AppLogs { get; set; } = null!;
        public DbSet<VisitLog> VisitLogs { get; set; } = null!;

        public DbSet<LoginLog> LoginLogs { get; set; } = null!;
        public DbSet<AppParameter> AppParameters { get; set; } = null!;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Create unique index on ParamKey
            modelBuilder.Entity<AppParameter>()
                .HasIndex(p => p.ParamKey)
                .IsUnique();
        }



        public async Task<Bkg_User?> GetBkgUserByIdAsync(int userId)
        {
            return await bkg_Users.FirstOrDefaultAsync(u => u.UserId == userId);
        }
        public async Task<Bkg_User?> GetBkgUserByAppUserIdAsync(string appUserId)
        {
            return await bkg_Users.FirstOrDefaultAsync(u => u.AppUserId == appUserId);
        }

        public async Task<bool> BookingAnyAsync(int userId)
        {
            return await bkg_Bookings.AnyAsync(u => u.UserId == userId);
        }
        public async Task<bool> ReviewAnyAsync(int userId)
        {
            return await bkg_Reviews.AnyAsync(r => r.UserId == userId);
        }
        public async Task<Bkg_User?> DeleteBkgUserAsync(int usrId)
        {
            var buser = await bkg_Users.FindAsync(usrId);
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
                               select role.Name ?? string.Empty).ToListAsync();

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
            return await bkg_Users.CountAsync();
        }
        public async Task<int> GetIdentityUserCountAsync()
        {
            return await ApplicationUsers.CountAsync();
        }
        public async Task<int> CountUserConfirmedAsync()
        {
            return await ApplicationUsers.CountAsync(u => u.EmailConfirmed);
        }
    }
}
