using System;
using System.Collections.Generic;
using System.Linq;

namespace ReportsGenerator.Model.Reports.Intermediate
{
    public class BasicReportsWalker
    {
        private readonly IDictionary<string, BasicReportsWalker> _childrenWalkers;
        private readonly IDictionary<DateTime, Metrics> _metrics;
        private readonly IDictionary<DateTime, BasicReport> _reports;
        private DateTime _lastDay;
        private DateTime _startDay;

        public BasicReportsWalker(IEnumerable<BasicReport> reports, StatsReport reportsStructure)
        {
            var grouppedReports = reports.GroupBy(r => r.Parent).ToArray();

            _reports = grouppedReports.FirstOrDefault(grp => grp.Key == string.Empty)?
                .ToDictionary(br => br.Day) ?? new Dictionary<DateTime, BasicReport>();
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
                            grouppedReports.First(grp => grp.Key == province).Where(br => br.Name == county),
                            countyChildren)
                    }).ToDictionary(nw => nw.Name, nw => nw.Walker);

                _childrenWalkers.Add(province, new BasicReportsWalker(
                    grouppedReports.First(grp => grp.Key == reportsStructure.Root.Name).Where(br => br.Name == province),
                    countyWalkers));
            }
        }

        private BasicReportsWalker(IEnumerable<BasicReport> reports, IDictionary<string, BasicReportsWalker> childrenWalkers)
        {
            _reports = reports.ToDictionary(br => br.Day);
            _metrics = new Dictionary<DateTime, Metrics>();
            _childrenWalkers = childrenWalkers;
        }

        public DateTime LastDay
        {
            get
            {
                if (_lastDay == DateTime.MinValue)
                    _lastDay = _reports.Keys.Concat(_childrenWalkers.Select(cw => cw.Value.LastDay)).Max();

                return _lastDay;
            }
        }

        public DateTime StartDay
        {
            get
            {
                if (_startDay == DateTime.MinValue)
                    _startDay = _reports.Keys.Concat(_childrenWalkers.Select(cw => cw.Value.StartDay)).Min();

                return _startDay;
            }
        }

        public IEnumerable<DateTime> GetAvailableDates() => _reports.Values.Select(r => r.Day);

        public Metrics GetCountryChangeForPeriod(DateTime startDay, int days) => GetChangeForPeriod(startDay, days, this);

        public Metrics GetCountryTotalByDay(DateTime day) => GetTotalByDay(day, this);

        public Metrics GetCountyChangeForPeriod(string province, string county, DateTime startDay, int days) =>
            GetCountyWalker(province, county, out var countyWalker)
                ? GetChangeForPeriod(startDay, days, countyWalker!)
                : Metrics.Empty;

        public Metrics GetCountyTotalByDay(string province, string county, DateTime day) =>
                    GetCountyWalker(province, county, out var countyWalker)
                ? GetTotalByDay(day, countyWalker!)
                : Metrics.Empty;

        public Metrics GetProvinceChangeForPeriod(string province, DateTime startDay, int days) =>
            GetProvinceWalker(province, out var provinceWalker)
                ? GetChangeForPeriod(startDay, days, provinceWalker!, FilterTerritory(province, this))
                : Metrics.Empty;

        public Metrics GetProvinceTotalByDay(string province, DateTime day) =>
                    GetProvinceWalker(province, out var provinceWalker)
                ? GetTotalByDay(day, provinceWalker!, FilterTerritory(province, this))
                : Metrics.Empty;

        private static bool FilterTerritory(string name, BasicReportsWalker walker) =>
            name == Consts.MainCountryRegion && walker._childrenWalkers.Count > 1 ||
            name == Consts.OtherCountryRegion;

        private static Metrics GetChangeForPeriod(
            DateTime startDay,
            int days,
            BasicReportsWalker walker,
            bool emptyIfNotExist = false) =>
            days < 1
                ? throw new ArgumentOutOfRangeException(nameof(days), days, "Days must be greater than 0 (zero).")
                : GetTotalByDay(startDay.AddDays(days - 1), walker, emptyIfNotExist) -
                  GetTotalByDay(startDay.AddDays(-1), walker, emptyIfNotExist);

        private static Metrics GetTotalByDay(DateTime day, BasicReportsWalker walker, bool emptyIfNotExist = false)
        {
            if (walker._metrics.ContainsKey(day)) // Check metrics cache
                return walker._metrics[day];

            Metrics? metrics = null;

            if (walker._reports.ContainsKey(day)) // Check reports data
                metrics = walker._reports[day].Total;

            if (emptyIfNotExist)
            {
                if (metrics == null)
                {
                    metrics = walker.StartDay <= day && day <= walker.LastDay
                        ? GetTotalByDay(day.AddDays(-1), walker, true)
                        : Metrics.Empty;
                }
            }
            else
            {
                //Didn't found anything, let's try to look deeper...
                if (metrics == Metrics.Empty || metrics == null)
                {
                    metrics = walker._childrenWalkers
                        .Select(cwKvp =>
                            GetTotalByDay(day, cwKvp.Value, FilterTerritory(cwKvp.Key, walker)))
                        .Aggregate(Metrics.Empty, (sum, dayM) => sum + dayM);
                }

                // Still no results, let's reuse previous day data...
                if (metrics == Metrics.Empty && day > walker.StartDay)
                    metrics = GetTotalByDay(day.AddDays(-1), walker);
            }

            walker._metrics.Add(day, metrics);
            return metrics;
        }

        private bool GetCountyWalker(string province, string county, out BasicReportsWalker? countyWalker)
        {
            countyWalker = null;
            return GetProvinceWalker(province, out var provinceWalker) && provinceWalker!._childrenWalkers.TryGetValue(county, out countyWalker);
        }

        private bool GetProvinceWalker(string province, out BasicReportsWalker? provinceWalker) =>
                                            _childrenWalkers.TryGetValue(province, out provinceWalker);
    }
}