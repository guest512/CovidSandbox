using System;
using System.Collections.Generic;
using System.Globalization;
using ReportsGenerator.Model;
using ReportsGenerator.Model.Reports;

namespace ReportsGenerator.Data
{
    /// <summary>
    /// Represents a trivial implementation of <see cref="IReportFormatter"/> interface.
    /// </summary>
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

        /// <inheritdoc />
        public IEnumerable<string> GetData(DayReport report, string country)
        {
            yield return country;

            foreach (var data in GetMetricsData(report.GetCountryTotal(country), report.GetCountryChange(country)))
            {
                yield return data;
            }
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public IEnumerable<string> GetData(StatsReportNode report)
        {
            yield return report.Name;
            yield return report.Continent;
            yield return report.Population.ToString("D");
        }

        /// <inheritdoc />
        public IEnumerable<string> GetHeader<T>()
        {
            if (typeof(T) == typeof(DayReport))
                return _dayByDayReportHeader;

            if (typeof(T) == typeof(BaseCountryReport))
                return _countryReportHeader;

            if (typeof(T) == typeof(StatsReportNode))
                return _statsReportHeader;

            throw new InvalidOperationException("Unknown report type.");
        }

        /// <inheritdoc />
        public IEnumerable<string> GetName(DayReport report)
        {
            return new[] { report.Day.ToString("yyyy-MM-dd") };
        }

        /// <inheritdoc />
        public IEnumerable<string> GetName(BaseCountryReport report, string? parent)
        {
            return string.IsNullOrEmpty(parent) ? new[] { report.Name } : new[] { parent, report.Name };
        }

        /// <inheritdoc />
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