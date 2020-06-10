using System;
using System.Collections.Generic;
using System.Linq;

namespace CovidSandbox.Model
{
    public static class Utils
    {
        public static DateTime PandemicStart { get; } = new DateTime(2020, 1, 1);

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