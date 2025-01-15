using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace RVParking.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<ApplicationUser>? ApplicationUsers { get; set; } 
        public DbSet<Bkg_Availability>? bkg_Availabilities { get; set; }
        public DbSet<Bkg_Booking>? bkg_Bookings { get; set; }
        public DbSet<Bkg_Payment>? bkg_Payments { get; set; }
        public DbSet<Bkg_Property>? bkg_Properties { get; set; }
        public DbSet<Bkg_User>? bkg_Users { get; set; }
        public DbSet<Bkg_Review>? bkg_Reviews { get; set; }
        public DbSet<Contact>? contacts { get; set; }

    }
}
