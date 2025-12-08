namespace RVParking.Data
{
    public class VisitLog
    {
        public int Id { get; set; }
        public string Path { get; set; }
        public string QueryString { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? UserId { get; set; } // Nullable FK to Identity User
        public DateTimeOffset Timestamp { get; set; }
    }

}
