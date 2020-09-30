using System;
using System.Globalization;

namespace ReportsGenerator.Utils
{
    public static class Convertors
    {
        public static DateTime ParseDate(string dateString)
        {
            var formats = new[]
            {
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

            if (string.IsNullOrEmpty(dateString))
                return DateTime.MinValue.Date;

            if (DateTime.TryParseExact(dateString, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                return date.Date;

            throw new ArgumentException("Unsupported DateTime format", nameof(dateString));
        }

        public static string ToCsvString(this string str) => str.Contains(',') ? $"\"{str}\"" : str;
    }
}