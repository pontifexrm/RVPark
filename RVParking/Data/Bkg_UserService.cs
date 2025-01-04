using Microsoft.EntityFrameworkCore;
using RVParking.Data;

public class Bkg_UserService
{
    private readonly ApplicationDbContext _context;

    public Bkg_UserService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Bkg_User?> GetBkgUserByIdAsync(int userId)
    {
        return await _context.bkg_Users.FirstOrDefaultAsync(u => u.UserId == userId);
    }

    public async Task<Bkg_User?> GetBkgUserByAppUserIdAsync(string appUserId)
    {
        return await _context.bkg_Users.FirstOrDefaultAsync(u => u.AppUserId == appUserId);
    }
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

}
