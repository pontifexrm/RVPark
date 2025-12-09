namespace RVParking.Data
{
    public class VisitStats
    {
        public string Location { get; set; }
        public int Count24h { get; set; }

        public int CountWeek { get; set; }
        public int CountMonth { get; set; }
        public int CountYear { get; set; }
    }
}
