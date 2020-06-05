using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace CovidSandbox.Data
{
    internal static class Utils
    {
        public static string ToCsvString(this string str) => str.Contains(',') ? $"\"{str}\"" : str;

        public static DateTime ParseDate(string dateString)
        {
            var formats = new[]
            {
                "M/dd/yyyy HH:mm",
                "M/dd/yy HH:mm",
                "M/d/yyyy HH:mm",
                "M/d/yyyy H:mm",
                "yyyy-MM-ddTHH:mm:ss",
                "M/d/yy H:mm",
                "yyyy-MM-dd HH:mm:ss"
            };

            if (DateTime.TryParseExact(dateString, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                return date.Date;

            throw new ArgumentException("Unsupported DateTime format", nameof(dateString));
        }

        
    }
}
