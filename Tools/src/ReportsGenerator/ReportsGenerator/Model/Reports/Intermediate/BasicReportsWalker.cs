using System;
using System.Collections.Generic;
using System.Linq;

namespace ReportsGenerator.Model.Reports.Intermediate
{
    public class BasicReportsWalker
    {
        private readonly IDictionary<string, BasicReportsWalker> _childrenWalkers;
        private readonly IDictionary<DateTime, BasicReport> _reports;
        private readonly IDictionary<DateTime, Metrics> _metrics;

        public BasicReportsWalker(IEnumerable<BasicReport> reports, StatsReport reportsStructure)
        {
            reports = reports as BasicReport[] ?? reports.ToArray();

            _reports = reports.Where(br => br.Name == reportsStructure.Root.Name)
                .ToDictionary(br => br.Day);
            _metrics = new Dictionary<DateTime, Metrics>();

            _childrenWalkers = new Dictionary<string, BasicReportsWalker>();
            var countyChildren = new Dictionary<string, BasicReportsWalker>();

            foreach (var province in reportsStructure.GetProvinces())
            {
                var countyWalkers =
                    reportsStructure.GetCounties(province).Select(county => new
                    {
                        Name = county,
                        Walker = new BasicReportsWalker(
                            reports.Where(br => br.Name == county && br.Parent == province),
                            countyChildren)
                    }).ToDictionary(nw => nw.Name, nw => nw.Walker);

                _childrenWalkers.Add(province, new BasicReportsWalker(
                    reports.Where(br => br.Name == province),
                    countyWalkers));
            }
        }

        private BasicReportsWalker(IEnumerable<BasicReport> reports, IDictionary<string, BasicReportsWalker> childrenWalkers)
        {
            _reports = reports.ToDictionary(br => br.Day);
            _metrics = new Dictionary<DateTime, Metrics>();
            _childrenWalkers = childrenWalkers;
        }

        public DateTime LastDay => _reports.Keys.Last();

        public DateTime StartDay => _reports.Keys.First();

        public IEnumerable<DateTime> GetAvailableDates() => _reports.Values.Select(r => r.Day);

        public Metrics GetCountryMetricsForDay(DateTime day) => GetCountryMetricsForPeriod(day, 1);

        public Metrics GetCountryMetricsForPeriod(DateTime startDay, int days) =>
            GetTotalForPeriod(startDay, days, this);

        public Metrics GetCountyMetricsForDay(string province, string county, DateTime day) =>
            GetCountyMetricsForPeriod(province, county, day, 1);

        public Metrics GetCountyMetricsForPeriod(string province, string county, DateTime startDay, int days) =>
            _childrenWalkers.TryGetValue(province, out var provinceWalker) &&
            provinceWalker._childrenWalkers.TryGetValue(county, out var countyWalker)
                ? GetTotalForPeriod(startDay, days, countyWalker)
                : Metrics.Empty;

        public Metrics GetProvinceMetricsForDay(string province, DateTime day) =>
            GetProvinceMetricsForPeriod(province, day, 1);

        public Metrics GetProvinceMetricsForPeriod(string province, DateTime startDay, int days) =>
            _childrenWalkers.TryGetValue(province, out var provinceWalker)
                ? GetTotalForPeriod(startDay, days, provinceWalker,
                    province == Consts.MainCountryRegion && _childrenWalkers.Count > 1)
                : Metrics.Empty;

        private static Metrics GetTotalForDay(DateTime day, BasicReportsWalker walker, bool emptyIfNotExist)
        {
            if (walker._metrics.ContainsKey(day))
                return walker._metrics[day];

            if (walker._reports.ContainsKey(day))
                walker._metrics.Add(day, walker._reports[day].Total);
            else if (emptyIfNotExist)
                walker._metrics.Add(day, Metrics.Empty);
            else
            {
                walker._metrics.Add(
                    day,
                    walker._childrenWalkers.Values
                        .Select(cw => GetTotalForDay(day, cw, false))
                        .Aggregate(Metrics.Empty, (sum, dayM) => sum + dayM));
            }

            return walker._metrics[day];
        }

        private static Metrics GetTotalForPeriod(
            DateTime startDay,
            int days,
            BasicReportsWalker walker,
            bool emptyIfNotExist = false) =>
            days < 1
                ? throw new ArgumentOutOfRangeException(nameof(days), days, "Days must be greater than 0 (zero).")
                : new[] { startDay, startDay.AddDays(days - 1) }
                    .GetContinuousDateRange()
                    .Select(day => GetTotalForDay(day, walker, emptyIfNotExist))
                    .Aggregate(Metrics.Empty, (total, day) => total + day);
    }
}