using System;

namespace BingWallpaper.Helper
{
    public static class DateHelper
    {
        public static string Format(DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd");
        }

        public static DateTime Today => DateTime.UtcNow;
    }
}