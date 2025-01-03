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
}
