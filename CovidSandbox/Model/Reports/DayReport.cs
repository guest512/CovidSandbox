using CovidSandbox.Model.Reports.Intermediate;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CovidSandbox.Model.Reports
{
    public class DayReport
    {
        public DateTime Day { get; }
        public IEnumerable<string> AvailableCountries { get; }

        private readonly IDictionary<string, LinkedReport> _reports;

        public DayReport(in DateTime day, IDictionary<string,LinkedReport> reports)
        {
            Day = day;
            _reports = reports;
            AvailableCountries = _reports.Select(rep => rep.Value.Name).Distinct().ToArray();
        }

        public Metrics GetCountryTotal(string countryName)
        {
            var country = _reports[countryName];
            while (country.Next.Day <= Day && country.Next != LinkedReport.Empty)
                country = country.Next;

            return country.Total;
        }

        public Metrics GetCountryChange(string countryName)
        {
            var country = _reports[countryName];
            while (country.Next.Day <= Day && country.Next != LinkedReport.Empty)
                country = country.Next;

            return country.Change;
        }
    }
}