using System;
using System.Collections.Generic;
using System.Linq;

namespace ReportsGenerator.Model
{
    public static class Utils
    {
        public static IEnumerable<DateTime> GetContinuousDateRange(IEnumerable<DateTime> dates)
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
    }
}