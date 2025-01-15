using Microsoft.EntityFrameworkCore;
using RVParking.Data;

public class Bkg_UserService
{
    private readonly ApplicationDbContext _context;

    public Bkg_UserService(ApplicationDbContext context)
    {
        _context = context;
    }

    //public async Task<Bkg_User?> GetBkgUserByIdAsync(int userId)
    //{
    //    return await _context.bkg_Users.FirstOrDefaultAsync(u => u.UserId == userId);
    //}

    //public async Task<Bkg_User?> GetBkgUserByAppUserIdAsync(string appUserId)
    //{
    //    return await _context.bkg_Users.FirstOrDefaultAsync(u => u.AppUserId == appUserId);
    //}
    //AnyAsync(b => b.UserID == user.Id);  
    public async Task<bool> BookingAnyAsync(int userId)
    {
        return await (_context.bkg_Bookings?.AnyAsync(u => u.UserId == userId) ?? Task.FromResult(false));
    }
    public async Task<bool> ReviewAnyAsync(int userId)
    {
        return await (_context.bkg_Reviews?.AnyAsync(r => r.UserId == userId) ?? Task.FromResult(false));
    }
    public async Task<Bkg_User?> DeleteBkgUserAsync(int usrId)
    {
        var buser = await _context.bkg_Users.FindAsync(usrId);
        if (buser == null)
        {
            return null;
        }

        _context.bkg_Users.Remove(buser);
        await _context.SaveChangesAsync();
        return buser;
    }

    public bool Bkg_AvailableAllSync(DateTime Fmdate, DateTime Todate)
    {
        if (_context.bkg_Availabilities == null)
        {
            return false;
        }
        bool res = _context.bkg_Availabilities
            .Where(b => b.DateAvailable >= Fmdate && b.DateAvailable <= Todate)
            .All(b => b.Available);
        return  res;
        // TODO: this return true even where there are NO records in the table.

    }
    //public async Task<List<string>> GetUserRolesAsync(string userId)
    //{
    //    var roles = await (from userRole in _context.UserRoles
    //                       join role in _context.Roles on userRole.RoleId equals role.Id
    //                       where userRole.UserId == userId
    //                       select role.Name).ToListAsync();

    //    return roles;
    //}
    //public async Task RemoveUserRoleAsync(string userId, string roleId)
    //{
    //    var userRole = await _context.UserRoles
    //        .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

    //    if (userRole != null)
    //    {
    //        _context.UserRoles.Remove(userRole);
    //        await _context.SaveChangesAsync();
    //    }
    //}
    //public async Task AddUserRoleAsync(string userId, string roleId)
    //{
    //    var userRole = await _context.UserRoles
    //        .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

    //    if (userRole != null)
    //    {
    //        _context.UserRoles.Remove(userRole);
    //        await _context.SaveChangesAsync();
    //    }
    //}
}
