using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RVPark.Data
{
    public class Bkg_Availability
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AvailabilityId { get; set; }
        public int Bkg_PropertyId { get; set; }
        public int BookingId { get; set; }
        public DateTime DateAvailable { get; set; }
        public decimal Price { get; set; } = 0m;
        public bool Available { get; set; } = true;
        public string AvailabilityStatus { get; set; } = string.Empty;
        public Bkg_Property? Bkg_Property { get; set; }


    }
}
