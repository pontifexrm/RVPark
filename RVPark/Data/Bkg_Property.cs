using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RVPark.Data
{
    public class Bkg_Property
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PropertyId { get; set; }
        [Required(ErrorMessage = "Name is required")] 
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Zip { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public decimal PricePerPeriod { get; set; } = 0m;
        public int MaxGuestNbr { get; set; } = 1;
        public decimal MaxRVLength { get; set; } = 8m;
        public decimal MaxParkWidth { get; set; } = 3.8m;

        public bool HasPottableWater { get; set; } = false;
        public bool HasGreyWaterWaste { get; set; } = false;
        public bool HasSewer { get; set; } = false;
        public bool HasPower { get; set; } = false;
        public int MaxAmp { get; set; } = 10;
        public bool HasWifi { get; set; } = false;
        public bool HasEVCharge { get; set; } = false;
        public string EVChargeType { get; set; } = string.Empty;
        public bool HasEBikeCharge { get; set; } = false;
        public string PetSituation { get; set; } = string.Empty;
        public bool HasBathroom { get; set; } = false;
        public bool HasLaundry { get; set; } = false;
        public bool HasKitchenette { get; set; } = false;
        public bool HasBedroom { get; set; } = false;
        public bool HasLivingArea { get; set; } = false;
        public bool HasOffStreetCarPark { get; set; } = false;
        public string KitchenDescription { get; set; } = string.Empty;
        public string BedroomDescription { get; set; } = string.Empty;
        public string BathroomDescription { get; set; } = string.Empty;
        public string LivingAreaDescription { get; set; } = string.Empty;
        public bool Enabled { get; set; } = true;

        public ICollection<Bkg_Availability>? Bkg_Availabilities { get; set; }
        public ICollection<Bkg_Booking>? Bkg_Bookings { get; set; }  

        public Bkg_User? Bkg_User { get; set; }


    }
}
