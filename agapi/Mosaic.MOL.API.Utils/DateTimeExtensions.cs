using System;

namespace Mosaic.MOL.API.Utils
{
    public static class DateTimeExtensions
    {
        public static DateTime LastDayOfMonth(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
        }
    }
}
