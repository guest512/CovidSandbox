using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ReportsGenerator.Model.Reports.Intermediate
{
    /// <summary>
    /// Represents a class to walk through bunch of <see cref="BasicReport"/> and calculate Total or Change metrics for day or period.
    /// </summary>
    public class BasicReportsWalker
    {
        private readonly IDictionary<string, BasicReportsWalker> _childrenWalkers;
        private readonly IDictionary<DateTime, Metrics> _metricsCache;
        private readonly SemaphoreSlim _metricsCacheLocker;
        private readonly IDictionary<DateTime, BasicReport> _reports;
        private DateTime _lastDay;
        private DateTime _startDay;

        /// <summary>
        /// Returns collection of <see cref="BasicReport"/> used by this <see cref="BasicReportsWalker"/>.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="BasicReport"/>.</returns>
        public IEnumerable<BasicReport> DumpReports()
        {
            foreach (var report in _reports)
            {
                yield return report.Value;
            }

            foreach (var report in _childrenWalkers.SelectMany(cw => cw.Value.DumpReports()))
            {
                yield return report;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicReportsWalker"/> class.
        /// </summary>
        /// <param name="reports"><see cref="BasicReport"/> collection to use for data extraction.</param>
        /// <param name="reportsStructure"><see cref="StatsReport"/> for the country to know how to walk across reports.</param>
        public BasicReportsWalker(IEnumerable<BasicReport> reports, StatsReport reportsStructure) : this()
        {
            var grouppedReports = reports.GroupBy(r => r.Parent).ToArray();

            _reports = grouppedReports.FirstOrDefault(grp => grp.Key == string.Empty)?
                .ToDictionary(br => br.Day) ?? new Dictionary<DateTime, BasicReport>();

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

        private BasicReportsWalker(IEnumerable<BasicReport> reports, IDictionary<string, BasicReportsWalker> childrenWalkers) : this()
        {
            _reports = reports.ToDictionary(br => br.Day);
            _childrenWalkers = childrenWalkers;
        }

        private BasicReportsWalker()
        {
            _reports = new Dictionary<DateTime, BasicReport>();
            _childrenWalkers = new Dictionary<string, BasicReportsWalker>();
            _metricsCache = new Dictionary<DateTime, Metrics>();
            _metricsCacheLocker = new SemaphoreSlim(1, 1);
        }

        /// <summary>
        /// Gets the last available day for this <see cref="BasicReportsWalker"/>. All metrics for later days are copies of the metrics for the LastDay.
        /// </summary>
        public DateTime LastDay
        {
            get
            {
                if (_lastDay == DateTime.MinValue)
                    _lastDay = _reports.Keys.Concat(_childrenWalkers.Select(cw => cw.Value.LastDay)).Max();

                return _lastDay;
            }
        }

        /// <summary>
        /// Gets the first available day for this <see cref="BasicReportsWalker"/>. All metrics for earlier days are <see cref="Metrics.Empty"/>.
        /// </summary>
        public DateTime StartDay
        {
            get
            {
                if (_startDay == DateTime.MinValue)
                    _startDay = _reports.Keys.Concat(_childrenWalkers.Select(cw => cw.Value.StartDay)).Min();

                return _startDay;
            }
        }

        /// <summary>
        /// Gets <see cref="Metrics"/> with 'Change' values for the specified period for the country.
        /// </summary>
        /// <param name="startDay">Period start date.</param>
        /// <param name="days">Period length.</param>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="days"/> value is less than zero.</exception>
        /// <returns>A <see cref="Metrics"/> object with 'Change' values.</returns>
        public Metrics GetCountryChangeForPeriod(DateTime startDay, int days) => GetChangeForPeriod(startDay, days, this);

        /// <summary>
        /// Gets <see cref="Metrics"/> with 'Total' values for the specified day for the country.
        /// </summary>
        /// <param name="day">Day to calculate Total.</param>
        /// <returns>A <see cref="Metrics"/> object with 'Total' values.</returns>
        public Metrics GetCountryTotalByDay(DateTime day) => GetTotalByDay(day, this);

        /// <summary>
        /// Gets <see cref="Metrics"/> with 'Change' values for the specified period for the county.
        /// </summary>
        /// <param name="province">County province.</param>
        /// <param name="county">County name.</param>
        /// <param name="startDay">Period start date.</param>
        /// <param name="days">Period length.</param>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="days"/> value is less than zero.</exception>
        /// <returns>A <see cref="Metrics"/> object with 'Change' values.</returns>
        public Metrics GetCountyChangeForPeriod(string province, string county, DateTime startDay, int days) =>
            GetCountyWalker(province, county, out var countyWalker)
                ? GetChangeForPeriod(startDay, days, countyWalker!)
                : Metrics.Empty;

        /// <summary>
        /// Gets <see cref="Metrics"/> with 'Total' values for the specified day for the county.
        /// </summary>
        /// <param name="province">County province.</param>
        /// <param name="county">County name.</param>
        /// <param name="day">Day to calculate Total.</param>
        /// <returns>A <see cref="Metrics"/> object with 'Total' values.</returns>
        public Metrics GetCountyTotalByDay(string province, string county, DateTime day) =>
                    GetCountyWalker(province, county, out var countyWalker)
                ? GetTotalByDay(day, countyWalker!)
                : Metrics.Empty;

        /// <summary>
        /// Gets <see cref="Metrics"/> with 'Change' values for the specified period for the province.
        /// </summary>
        /// <param name="province">Province name.</param>
        /// <param name="startDay">Period start date.</param>
        /// <param name="days">Period length.</param>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="days"/> value is less than zero.</exception>
        /// <returns>A <see cref="Metrics"/> object with 'Change' values.</returns>
        public Metrics GetProvinceChangeForPeriod(string province, DateTime startDay, int days) =>
            GetProvinceWalker(province, out var provinceWalker)
                ? GetChangeForPeriod(startDay, days, provinceWalker!, FilterTerritory(province, this))
                : Metrics.Empty;

        /// <summary>
        /// Gets <see cref="Metrics"/> with 'Total' values for the specified day for the province.
        /// </summary>
        /// <param name="province">Province name.</param>
        /// <param name="day">Day to calculate Total.</param>
        /// <returns>A <see cref="Metrics"/> object with 'Total' values.</returns>
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
            // This method contains core business logic for the walker.
            // The idea is simple - looking for data (metrics) in available reports from data sources
            // If metrics not exist, or empty (all zeroes), then try to obtain it from child reports,
            // or use metrics from previous day.
            //
            // There are some exceptions in this logic that defined by the flag 'emptyIfNotExist'.
            // If this flag set, then we don't try to use child reports for the data, and also,
            // if day is out of range [StartDay, LastDay], then we don't reuse metrics
            // from previous days, and return Metrics.Empty instead.
            //
            // To improve speed of this function, the metrics cache is used. Since the function
            // used in multi threaded environment, we check cache several times, before every
            // potentially long-time operation.

            var metrics = walker.GetMetricsFromCache(day);

            if (metrics != null)
                return metrics;

            if (walker._reports.ContainsKey(day)) // Check reports data
                metrics = walker._reports[day].Total;

            if (emptyIfNotExist)
            {
                if (metrics == null)
                {
                    metrics = walker.StartDay <= day && day <= walker.LastDay
                        ? walker.GetMetricsFromCache(day) ?? GetTotalByDay(day.AddDays(-1), walker, true)
                        : Metrics.Empty;
                }
            }
            else
            {
                //Didn't found anything, let's try to look deeper...
                if (metrics == Metrics.Empty || metrics == null)
                {
                    metrics = walker.GetMetricsFromCache(day) ?? walker._childrenWalkers
                        .Select(cwKvp =>
                            GetTotalByDay(day, cwKvp.Value, FilterTerritory(cwKvp.Key, walker)))
                        .Aggregate(Metrics.Empty, (sum, dayM) => sum + dayM);
                }

                // Still no results, let's reuse previous day data...
                if (metrics == Metrics.Empty && day > walker.StartDay)
                    metrics = walker.GetMetricsFromCache(day) ?? GetTotalByDay(day.AddDays(-1), walker);
            }

            walker.AddMetricsToCacheIfNotExist(day, metrics);
            return metrics;
        }

        private void AddMetricsToCacheIfNotExist(DateTime day, Metrics metrics)
        {
            try
            {
                _metricsCacheLocker.Wait();

                // There are no reasons to think that metrics value
                // can differ for the same day. So, we simply ignore
                // new values and don't add them to cache.
                if (_metricsCache.ContainsKey(day))
                    return;

                _metricsCache.Add(day, metrics);
            }
            finally
            {
                _metricsCacheLocker.Release();
            }
        }

        private bool GetCountyWalker(string province, string county, out BasicReportsWalker? countyWalker)
        {
            countyWalker = null;
            return GetProvinceWalker(province, out var provinceWalker) && provinceWalker!._childrenWalkers.TryGetValue(county, out countyWalker);
        }

        private Metrics? GetMetricsFromCache(DateTime day)
        {
            try
            {
                _metricsCacheLocker.Wait();
                return _metricsCache.ContainsKey(day) ? _metricsCache[day] : null;
            }
            finally
            {
                _metricsCacheLocker.Release();
            }
        }

        private bool GetProvinceWalker(string province, out BasicReportsWalker? provinceWalker) =>
                                            _childrenWalkers.TryGetValue(province, out provinceWalker);
    }
}