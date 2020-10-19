using System;
using System.Globalization;

namespace ReportsGenerator.Utils
{
    public static class Convertors
    {
        private static ILogger _logger = new NullLogger();

        private static readonly string[] Formats =
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

        public static void SetLogger(ILogger logger)
        {
            _logger = logger;
        }

        public static DateTime AsDate(this string dateString)
        {
            if (string.IsNullOrEmpty(dateString))
                return DateTime.MinValue.Date;

            if (DateTime.TryParseExact(dateString, Formats, CultureInfo.InvariantCulture, DateTimeStyles.None,
                out var date))
                return date.Date;

            _logger.WriteError($"Unable to parse '{dateString}' as date.");

            throw new ArgumentException("Unsupported DateTime format", nameof(dateString));
        }

        public static long AsLong(this string stringValue, long defaultValue = 0)
        {
            var value = long.TryParse(stringValue, out var intValue) ? intValue : defaultValue;

            if (!stringValue.Contains('.'))
                return value;

            var floatValue = float.Parse(stringValue, CultureInfo.InvariantCulture);
            if (!(floatValue % 1 < float.Epsilon))
                return value;

            value = (long)floatValue;
            _logger.WriteWarning($"Possible data loss - float converted to long. '{floatValue}' -> '{value}'.");

            return value;
        }
    }
}