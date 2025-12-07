namespace RVParking.Data
{
    public class LoginLog
    {
        public int Id { get; set; }
        public string UserId { get; set; } // FK to Identity User
        public string IpAddress { get; set; }
        public bool Success { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }

}
