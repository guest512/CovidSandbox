using System;
using System.Globalization;

namespace ReportsGenerator.Utils
{
    public static class Convertors
    {
        private static readonly string[] Formats = {
            "MM-dd-yyyy",
            "M/dd/yyyy HH:mm",
            "M/d/yyyy HH:mm",
            "M/d/yyyy H:mm",

            "M/dd/yy HH:mm",
            "M/d/yy H:mm",

            "yyyy-MM-ddTHH:mm:ss",
            "yyyy-MM-dd HH:mm:ss",
            "dd.MM.yyyy"
        };

        public static DateTime ParseDate(string dateString)
        {
            if (string.IsNullOrEmpty(dateString))
                return DateTime.MinValue.Date;

            if (DateTime.TryParseExact(dateString, Formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                return date.Date;

            throw new ArgumentException("Unsupported DateTime format", nameof(dateString));
        }
    }
}