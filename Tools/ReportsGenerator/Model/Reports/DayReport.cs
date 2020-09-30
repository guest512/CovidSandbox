using CovidSandbox.Model.Reports.Intermediate;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CovidSandbox.Model.Reports
{
    public class DayReport
    {
        private readonly IDictionary<string, LinkedReport> _reports;

        public DayReport(in DateTime day, IDictionary<string, LinkedReport> reports)
        {
            Day = day;
            _reports = reports;
            AvailableCountries = _reports.Select(rep => rep.Value.Name).Distinct().ToArray();
        }

        public IEnumerable<string> AvailableCountries { get; }
        public DateTime Day { get; }

        public Metrics GetCountryChange(string countryName)
        {
            var country = _reports[countryName];
            while (country.Next.Day <= Day && country.Next != LinkedReport.Empty)
                country = country.Next;

            return country.Total - country.Previous.Total;
        }

        public Metrics GetCountryTotal(string countryName)
        {
            var country = _reports[countryName];
            while (country.Next.Day <= Day && country.Next != LinkedReport.Empty)
                country = country.Next;

            return country.Total;
        }
    }
}