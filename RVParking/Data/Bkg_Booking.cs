using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;

namespace RVParking.Data
{
    public class Bkg_Booking
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
        public int BookingId { get; set; }
        public int PropertyId { get; set; }
        public int UserId { get; set; }
        public DateTime DateArrive { get; set; }
        public DateTime DateDepart { get; set; }
        public decimal TotalPrice { get; set; } = 0m;
        public bool Paid { get; set; } = false;
        public string   BookingStatus { get; set; } = string.Empty;
        public string BookingComments { get; set; } = string.Empty;

        // Navigation properties
        public Bkg_User Bkg_User { get; set; }
        public Bkg_Property Bkg_Property { get; set; }
    }
}