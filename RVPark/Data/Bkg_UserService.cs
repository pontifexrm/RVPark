using Microsoft.EntityFrameworkCore;
namespace RVPark.Data
{
    public class Bkg_UserService
    {
        private readonly ApplicationDbContext _context;

        public Bkg_UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> BookingAnyAsync(int userId)
        {
            return await _context.bkg_Bookings.AnyAsync(u => u.UserId == userId);
        }
        public async Task<bool> ReviewAnyAsync(int userId)
        {
            return await _context.bkg_Reviews.AnyAsync(r => r.UserId == userId);
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
            bool res = _context.bkg_Availabilities
                .Where(b => b.DateAvailable >= Fmdate && b.DateAvailable <= Todate)
                .All(b => b.Available);
            return res;
            // TODO: this return true even where there are NO records in the table.

        }

    }
}
