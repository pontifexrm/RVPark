using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RVParking.Data
{
    public class Bkg_Property
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PropertyId { get; set; }
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
        public int MaxParkWidth { get; set; } = 04;
        public int MaxAmp { get; set; } = 10;
        public bool HasWater { get; set; } = false;
        public bool HasSewer { get; set; } = false;
        public bool HasElectric { get; set; } = false;
        public bool HasWifi { get; set; } = false;
        public bool HasEVCharge { get; set; } = false;
        public string EVChargeType { get; set; } = string.Empty;
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
        
        public ICollection<Bkg_Availability> Bkg_Availabilities { get; set; }
        public ICollection<Bkg_Booking> Bkg_Bookings { get; set; }  

        public Bkg_User? Bkg_User { get; set; }


    }
}
