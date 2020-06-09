using System;
using System.Collections.Generic;
using System.Linq;

namespace CovidSandbox.Model.Reports
{
    public class DayReport
    {
        public DateTime Day { get; }
        public IEnumerable<string> AvailableCountries { get; }

        private readonly IEnumerable<IntermidiateReport> _reports;

        public DayReport(in DateTime day, IEnumerable<IntermidiateReport> reports)
        {
            Day = day;
            _reports = reports.ToArray();
            AvailableCountries = _reports.Select(_ => _.Name).Distinct().ToArray();
        }

        public Metrics GetCountryTotal(string countryName) => _reports.FirstOrDefault(_ => _.Name == countryName).Total;

        public Metrics GetCountryChange(string countryName) => _reports.FirstOrDefault(_ => _.Name == countryName).Change;
    }
}