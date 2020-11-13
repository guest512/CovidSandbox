using System.Linq;
using System.Threading;
using ReportsGenerator.Data.IO;
using ReportsGenerator.Model.Reports;
using ReportsGenerator.Utils;

namespace ReportsGenerator.Data
{
    /// <summary>
    /// Represents a helper class to save reports using particular <see cref="IReportFormatter"/> and <see cref="IReportStorage"/>.
    /// </summary>
    public class ReportsSaver
    {
        private readonly IReportFormatter _formatter;
        private readonly ILogger _logger;
        private readonly IReportStorage _storage;
        private readonly SemaphoreSlim _statsLocker = new SemaphoreSlim(1,1);

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportsSaver"/> class.
        /// </summary>
        /// <param name="formatter">Desired report formatter to read reports.</param>
        /// <param name="storage">Desired storage to save reports data.</param>
        /// <param name="logger">A logger instance.</param>
        public ReportsSaver(IReportFormatter formatter, IReportStorage storage, ILogger logger)
        {
            _formatter = formatter;
            _storage = storage;
            _logger = logger;
        }

        /// <summary>
        /// Writes <see cref="CountryReport"/>.
        /// </summary>
        /// <param name="report">Report to write.</param>
        public void WriteReport(CountryReport report)
        {
            _logger.WriteInfo($"Write country: {report.Name}");
            WriteReport(report, null);

            foreach (var regionReport in report.RegionReports)
            {
                _logger.WriteInfo($"Write region: {report.Name} - {regionReport.Name}");
                WriteReport(regionReport, report.Name);
            }
        }

        /// <summary>
        /// Writes <see cref="DayReport"/>.
        /// </summary>
        /// <param name="report">Report to write.</param>
        public void WriteReport(DayReport report)
        {
            _logger.WriteInfo($"Write day: {report.Day:dd-MM-yyyy}");
            using var writer = _storage.GetWriter(_formatter.GetName(report), WriterType.Day);

            writer.WriteHeader(_formatter.GetHeader<DayReport>());
            foreach (var country in report.AvailableCountries)
            {
                _logger.WriteInfo($"Write day: {report.Day:dd-MM-yyyy} - {country}");
                writer.WriteDataLine(_formatter.GetData(report, country));
            }
        }

        /// <summary>
        /// Writes <see cref="StatsReport"/>.
        /// </summary>
        /// <param name="report">Report to write.</param>
        public void WriteStats(StatsReport report)
        {
            var root = report.Root;
            _logger.WriteInfo($"Write {root.Name} stats ...");

            _statsLocker.Wait();
            WriteStatReportNode(root);
            _statsLocker.Release();

            foreach (var province in root.Children.Where(child => child.Name != Consts.MainCountryRegion))
            {
                _logger.WriteInfo($"Write {root.Name}, {province.Name} stats ...");
                WriteStatReportNode(province);

                foreach (var county in province.Children)
                {
                    _logger.WriteInfo($"Write {root.Name}, {province.Name}, {county.Name} stats ...");
                    WriteStatReportNode(county);
                }
            }
        }

        private void WriteStatReportNode(StatsReportNode report)
        {
            using var writer = _storage.GetWriter(_formatter.GetName(report), WriterType.Stats);

            writer.WriteHeader(_formatter.GetHeader<StatsReportNode>());
            writer.WriteDataLine(_formatter.GetData(report));
        }

        private void WriteReport(BaseCountryReport report, string? parent)
        {
            using var writer = _storage.GetWriter(_formatter.GetName(report, parent), WriterType.Country);

            writer.WriteHeader(_formatter.GetHeader<BaseCountryReport>());
            foreach (var day in report.AvailableDates)
            {
                writer.WriteDataLine(_formatter.GetData(report, day));
            }
        }
    }
}