using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RVParking.Data
{
    public class Bkg_User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }
        public string AppUserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserFirstName { get; set; } = string.Empty;
        public string UserLastName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string UserPhone { get; set; } = string.Empty;
        public string UserNZMCA { get; set; } = string.Empty;
        public string UserAddress { get; set; } = string.Empty;
        public string UserCity { get; set; } = string.Empty;
        public string UserState { get; set; } = string.Empty;
        public string UserZip { get; set; } = string.Empty;
        public string UserCountry { get; set; } = string.Empty;
        public string UserStatus { get; set; } = string.Empty;
        public string UserPassword { get; set; } = string.Empty;
        public string UserComments { get; set; } = string.Empty;
        public DateTime CreatedDte { get; set; } = DateTime.Now;

        public ICollection<Bkg_Property> Bkg_Properties { get; set; }
        public ICollection<Bkg_Booking> Bkg_Bookings { get; set; }
        public ICollection<Bkg_Review> Bkg_Reviews { get; set; }
        public ICollection<Bkg_Payment> Bkg_Payments { get; set; }
    }
}
