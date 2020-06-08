using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace CovidSandbox.Data
{
    public static class Utils
    {
        public static string ToCsvString(this string str) => str !=null && str.Contains(',') ? $"\"{str}\"" : str;

        public static DateTime ParseDate(string dateString)
        {
            var formats = new[]
            {
                "M/dd/yyyy HH:mm",
                "M/d/yyyy HH:mm",
                "M/d/yyyy H:mm",

                "M/dd/yy HH:mm",
                "M/d/yy H:mm",

                "yyyy-MM-ddTHH:mm:ss",
                "yyyy-MM-dd HH:mm:ss"
            };

            if (DateTime.TryParseExact(dateString, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                return date.Date;

            throw new ArgumentException("Unsupported DateTime format", nameof(dateString));
        }

        
    }
}
