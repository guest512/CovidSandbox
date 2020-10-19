using ReportsGenerator.Model;
using ReportsGenerator.Model.Reports;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace ReportsGenerator.Data
{
    public class StringReportFormatter : IReportFormatter
    {
        private readonly string[] _countryReportHeader =
        {
            "Date",
            "Confirmed",
            "Active",
            "Recovered",
            "Deaths",
            "Confirmed_Change",
            "Active_Change",
            "Recovered_Change",
            "Deaths_Change",
            "Rt",
            "Time_To_Resolve"
        };

        private readonly string[] _dayByDayReportHeader =
        {
            "Country",
            "Confirmed",
            "Active",
            "Recovered",
            "Deaths",
            "Confirmed_Change",
            "Active_Change",
            "Recovered_Change",
            "Deaths_Change"
        };

        private readonly string[] _statsReportHeader =
        {
            "Name",
            "Continent",
            "Population"
        };

        public IEnumerable<string> GetData(DayReport report, string country)
        {
            yield return country;

            foreach (var data in GetMetricsData(report.GetCountryTotal(country), report.GetCountryChange(country)))
            {
                yield return data;
            }
        }

        public IEnumerable<string> GetData(BaseCountryReport report, DateTime day)
        {
            yield return day.ToString("dd-MM-yyyy");

            foreach (var data in GetMetricsData(report.GetDayTotal(day), report.GetDayChange(day)))
            {
                yield return data;
            }

            var rt = report.GetRt(day);
            yield return rt.ToString("00.00000000", CultureInfo.InvariantCulture);

            var ttr = report.GetTimeToResolve(day);
            yield return ttr.ToString("###########0", CultureInfo.InvariantCulture);
        }

        public IEnumerable<string> GetData(StatsReportNode report)
        {
            yield return report.Name;
            yield return report.Continent;
            yield return report.Population.ToString("D");
        }

        public IEnumerable<string> GetHeader(DayReport report)
        {
            return _dayByDayReportHeader;
        }

        public IEnumerable<string> GetHeader(BaseCountryReport report)
        {
            return _countryReportHeader;
        }

        public IEnumerable<string> GetHeader(StatsReportNode report)
        {
            return _statsReportHeader;
        }

        public IEnumerable<string> GetName(DayReport report)
        {
            return new[] { report.Day.ToString("yyyy-MM-dd") };
        }

        public IEnumerable<string> GetName(BaseCountryReport report, string? parent)
        {
            return string.IsNullOrEmpty(parent) ? new[] { report.Name } : new[] { parent, report.Name };
        }

        public IEnumerable<string> GetName(StatsReportNode report)
        {
            var name = new List<string>();
            while (report != StatsReportNode.Empty)
            {
                name.Add(report.Name);
                report = report.Parent;
            }

            name.Reverse();

            return name;
        }

        private static IEnumerable<string> GetMetricsData(params Metrics[] metrics)
        {
            foreach (var metric in metrics)
            {
                var (confirmed, active, recovered, deaths) = metric;
                yield return confirmed.ToString();
                yield return active.ToString();
                yield return recovered.ToString();
                yield return deaths.ToString();
            }
        }
    }
}