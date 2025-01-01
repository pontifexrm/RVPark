namespace RVParking.Data
{
    public class Bkg_Availability
    {
        public int AvailabilityId { get; set; }
        public int PropertyId { get; set; }
        public int BookingId { get; set; }
        public DateTime DateAvailable { get; set; }
        public decimal Price { get; set; } = 0m;
        public bool Available { get; set; } = true;
        public string AvailabilityStatus { get; set; } = string.Empty;

    }
}
