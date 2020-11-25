using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ReportsGenerator.Model;

namespace ReportsGenerator.Data
{
    /// <summary>
    /// Represents a trivial implementation of <see cref="IReportFormatter{TResult}"/> interface.
    /// </summary>
    public class CsvReportFormatter : IReportFormatter<string>
    {
        /// <inheritdoc />
        public IEnumerable<string> GetData<TRow, TName>(IFormattableReport<TRow, TName> report, TRow row)
        {
            foreach (var propName in report.Properties)
            {
                switch (report.GetValue(propName, row))
                {
                    case DateTime day:
                        yield return day.ToString("dd-MM-yyyy");
                        break;

                    case Metrics metrics:
                        foreach (var data in GetMetricsData(metrics))
                        {
                            yield return data;
                        }

                        break;

                    case double doubleValue:
                        yield return propName switch
                        {
                            "Rt" => doubleValue.ToString("00.00000000", CultureInfo.InvariantCulture),
                            "TTR" => doubleValue.ToString("###########0", CultureInfo.InvariantCulture),
                            _ => throw new ArgumentException($"Unsupported formatter for property value. Property - {propName}; Value - {doubleValue}")
                        };

                        break;

                    case string stringValue:
                        yield return stringValue;
                        break;

                    case long longValue:
                        yield return longValue.ToString("D");
                        break;

                    default:
                        throw new ArgumentException($"Unsupported formatter for property. Property - {propName}");
                }
            }
        }

        /// <inheritdoc />
        public IEnumerable<string> GetHeader<TRow, TName>(IFormattableReport<TRow, TName> report)
        {
            foreach (var prop in report.Properties)
            {
                switch (prop)
                {
                    case "Total":
                        yield return "Confirmed";
                        yield return "Active";
                        yield return "Recovered";
                        yield return "Deaths";
                        break;

                    case "Change":
                        yield return "Confirmed_Change";
                        yield return "Active_Change";
                        yield return "Recovered_Change";
                        yield return "Deaths_Change";
                        break;

                    case "TTR":
                        yield return "Time_To_Resolve";
                        break;

                    default:
                        yield return prop;
                        break;
                }
            }
        }

        /// <inheritdoc />
        public IEnumerable<string> GetName<TRow, TName>(IFormattableReport<TRow, TName> report)
        {
            return report.Name switch
            {
                IEnumerable<DateTime> dates => dates.Select(d => d.ToString("yyyy-MM-dd")),
                IEnumerable<string> names => names,
                _ => throw new ArgumentOutOfRangeException(nameof(TName))
            };
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