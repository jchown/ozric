using System;

namespace OzricEngine.ext
{
    public static class DateTimeExt
    {
        public static DateTime SetDayOfYear(this DateTime dateTime, int dayOfYear)
        {
            var diff = dayOfYear - dateTime.DayOfYear;
            if (Math.Abs(diff) > 182)   // Try not to change year
            {
                diff = dateTime.DayOfYear - dayOfYear;
                return dateTime.AddDays(-diff);
            }

            return dateTime.AddDays(diff);
        }
    }
}