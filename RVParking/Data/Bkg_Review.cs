namespace RVParking.Data
{
    public class Bkg_Review
    {
        public int ReviewId { get; set; }
        public int PropertyId { get; set; }
        public int UserId { get; set; }
        public DateTime ReviewDate { get; set; }
        public string ReviewText { get; set; } = string.Empty;
        public int Rating { get; set; } = 0;
    }
}
