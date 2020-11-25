using System;
using System.Collections.Generic;
using System.Linq;

namespace ReportsGenerator.Model
{
    /// <summary>
    /// Represents a collection of helper functions.
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Converts dates collection into the ordered continuous date range.
        /// </summary>
        /// <param name="dates">Dates collection to convert.</param>
        /// <returns>Continuous date range ordered from min to max values.</returns>
        public static IEnumerable<DateTime> GetContinuousDateRange(this IEnumerable<DateTime> dates)
        {
            dates = dates.ToArray();
            var minDate = dates.Min();
            var maxDate = dates.Max();
            var curDate = minDate;

            while (curDate <= maxDate)
            {
                yield return curDate;
                curDate = curDate.AddDays(1);
            }
        }

        /// <summary>
        /// A function that helps to process "province" from data file, when it contains county and province strings separated by comma.
        /// </summary>
        /// <param name="provinceRow">String to process.</param>
        /// <param name="county">Out parameter that contains a county string, if operation was successful.</param>
        /// <param name="state">Out parameter that contains a province (state) string, if operation was successful.</param>
        /// <returns>
        /// <see langword="true" /> if input row contains two rows separated by comma, otherwise returns <see langword="false" />.
        /// </returns>
        public static bool TrySplitStateToStateCounty(string provinceRow, out string county, out string state)
        {
            var values = provinceRow.Split(',');

            if (values.Length == 2)
            {
                county = values[0].Trim();
                state = values[1].Trim();
                return true;
            }

            county = state = string.Empty;
            return false;
        }
    }
}