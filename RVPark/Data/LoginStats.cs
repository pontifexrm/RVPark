namespace RVPark.Data
{
    public class LoginStats
    {
        public int FailedLast24Hours { get; set; }
        public int FailedLastWeek { get; set; }
        public int FailedLastMonth { get; set; }
        public int FailedLastYear { get; set; }

        public int SuccessLast24Hours { get; set; }
        public int SuccessLastWeek { get; set; }
        public int SuccessLastMonth { get; set; }
        public int SuccessLastYear { get; set; }


        public int TotalLoginAttempts24Hours
        {
            get
            {
                return FailedLast24Hours + SuccessLast24Hours;
            }
        }
        public int TotalLoginAttemptsWeek
        {
            get
            {
                return FailedLastWeek + SuccessLastWeek;
            }
        }
        public int TotalLoginAttemptsMonth
        {
            get
            {
                return FailedLastMonth + SuccessLastMonth;
            }
        }
        public int TotalLoginAttemptsYear
        {
            get
            {
                return FailedLastYear + SuccessLastYear;
            }
        }

    }
}
