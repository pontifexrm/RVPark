using System;


namespace RVParking.Data
{
    public static class DateHelper
    {
        public static DateTime GetCurrentDateInNZ()
        {
            // Find the New Zealand time zone
            TimeZoneInfo nzTimeZone = TimeZoneInfo.FindSystemTimeZoneById("New Zealand Standard Time");

            // Convert from UTC to NZ time
            DateTime utcNow = DateTime.UtcNow;
            DateTime nzNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, nzTimeZone);

            return nzNow.Date;
        }
    }

}
