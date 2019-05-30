using System;

namespace PortableClasses.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime ChangeTime(this DateTime value, TimeSpan time)
        {
            var dateInString = value.ToString("dd/MM/yyyy");
            var timeInString = (new DateTime(time.Ticks)).ToString("hh:mm:ss tt");
            return DateTime.ParseExact($"{dateInString} {timeInString}", "dd/MM/yyyy hh:mm:ss tt", null);
        }

        public static DateTime ChangeTime(this DateTime? value, TimeSpan time)
        {
            if (value is null) return DateTime.Now.ChangeTime(time);
            else
            {
                var dateInString = value?.ToString("dd/MM/yyyy");
                var timeInString = (new DateTime(time.Ticks)).ToString("hh:mm:ss tt");
                return DateTime.ParseExact($"{dateInString} {timeInString}", "dd/MM/yyyy hh:mm:ss tt", null);
            }
        }
    }
}
