namespace RVParking.Data
{
    public class Bkg_Payment
    {
        public int PaymentId { get; set; }
        public int UserId { get; set; }
        public int BookingId { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal PaymentAmount { get; set; } = 0m;
        public string PaymentType { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;

    }
}
