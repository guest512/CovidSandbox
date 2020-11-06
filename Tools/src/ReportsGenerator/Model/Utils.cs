using System;
using System.Collections.Generic;
using System.Linq;

namespace ReportsGenerator.Model
{
    public static class Utils
    {
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