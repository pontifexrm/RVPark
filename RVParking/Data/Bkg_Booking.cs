using System.Diagnostics.Contracts;

namespace RVParking.Data
{
    public class Bkg_Booking
    {
        public int BookingId { get; set; }
        public int PropertyId { get; set; }
        public int UserId { get; set; }
        public DateTime DateArrive { get; set; }
        public DateTime DateDepart { get; set; }
        public decimal TotalPrice { get; set; } = 0m;
        public bool Paid { get; set; } = false;
        public string   BookingStatus { get; set; } = string.Empty;

    }
}