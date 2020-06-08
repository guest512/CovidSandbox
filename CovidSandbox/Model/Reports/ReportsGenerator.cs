using System;
using System.Collections.Generic;
using System.Linq;

namespace CovidSandbox.Model.Reports
{
    public class MetricsCounter
    {
        private readonly ReportsGenerator _reportsGenerator = new ReportsGenerator();

        public MetricsCounter()
        {

        }

        public void AddEntries(IEnumerable<Entry> rows)
        {
            _reportsGenerator.AddEntries(rows);
        }

        public Dictionary<DateTime, Metrics> GetCountryMetricsTotal(string countryName, DateTime dayStart,
            DateTime dayEnd)
        {
            if (dayStart < Utils.PandemicStart)
                dayStart = Utils.PandemicStart;

            if (dayEnd < dayStart)
                dayEnd = dayStart;

            var day = dayStart;
            var countryReport = _reportsGenerator.GetCountryReport(countryName);

            coun

            while (day<= dayEnd)
            {
                
            }
        }
    }

    public class ReportsGenerator
    {
        private readonly List<Entry> _data = new List<Entry>();
        private readonly Dictionary<string, CountryReport> _countryReports = new Dictionary<string, CountryReport>();
        private readonly Dictionary<DateTime, DayReport> _dayReports = new Dictionary<DateTime, DayReport>();

        public ReportsGenerator()
        {
        }

        public void AddEntries(IEnumerable<Entry> rows)
        {
            var newRows = rows as Entry[] ?? rows.ToArray();
            _data.AddRange(newRows);

            foreach (var country in newRows.Select(_ => _.CountryRegion))
            {
                _countryReports.Remove(country);
            }

            foreach (var day in newRows.Select(_ => _.LastUpdate.Date))
            {
                _dayReports.Remove(day);
            }
        }

        public CountryReport GetCountryReport(string countryName)
        {
            if (_countryReports.ContainsKey(countryName))
                return _countryReports[countryName];

            var countryReport = new CountryReport(countryName, _data.Where(_ => _.CountryRegion == countryName));
            _countryReports.Add(countryName, countryReport);

            return _countryReports[countryName];
        }

        public DayReport GetDayReport(DateTime day)
        {
            day = day.Date;
            if (_dayReports.ContainsKey(day))
                return _dayReports[day];

            var dayReport = new DayReport(_data.Where(_=>_.LastUpdate == day),null);
            _dayReports.Add(day, dayReport);

            return _dayReports[day];
        }
    }

    public class DayReport : Report
    {
        private readonly DateTime _day;

        public IEnumerable<string> AvailableCountries { get; }

        public DayReport(IEnumerable<Entry> data, ReportsGenerator reportsGenerator) : base(data, reportsGenerator)
        {
            _day = _entries.First().LastUpdate.Date;
            AvailableCountries = _entries.Select(_ => _.CountryRegion).ToArray();
        }

        public Metrics GetTotalByCountry(string countryName)
        {
            return _reportsGenerator.GetCountryReportByDay(_day, countryName);
        }

        public Metrics GetDiffByCountry(string countryName)
        {
            return _reportsGenerator.GetCountryReportByDayDiff(_day, countryName);
        }
    }

    public abstract class Report
    {
        protected readonly IEnumerable<Entry> _entries;
        protected readonly ReportsGenerator _reportsGenerator;

        protected Report(IEnumerable<Entry> data, ReportsGenerator reportsGenerator)
        {
            _entries = data.ToArray();
            _reportsGenerator = reportsGenerator;
        }

        protected IEnumerable<Metrics> GetMetrics(Func<Entry, bool> predicate)
        {
            return _entries.Where(predicate).Select(Metrics.FromEntry);
        }
    }
}