namespace RVPark.Data
{
    public class VisitStats
    {
        public string? Location { get; set; }
        public int Count24h { get; set; }
        public int CountWeek { get; set; }
        public int CountMonth { get; set; }
        public int CountYear { get; set; }
        public int UniqueUsers24h { get; set; }
        public int UniqueUsersWeek { get; set; }
        public int UniqueUsersMonth { get; set; }
        public int UniqueUsersYear { get; set; }
    }
}
