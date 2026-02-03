namespace RVPark.Data
{
    // Need stats on user visits, similar to LoginStats and VisitStats and the location they visit and count.
    // If the User has not logged in the visit by IPAddress should be counted.

    public class UserVisitStats
    {
        public string UserNameIpAddr { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int Count24h { get; set; }
        public int CountWeek { get; set; }
        public int CountMonth { get; set; }
        public int CountYear { get; set; }
    }
}
