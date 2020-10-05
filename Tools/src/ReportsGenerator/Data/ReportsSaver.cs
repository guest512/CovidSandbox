using ReportsGenerator.Data.IO;
using ReportsGenerator.Model.Reports;
using ReportsGenerator.Utils;

namespace ReportsGenerator.Data
{
    public class ReportsSaver
    {
        private readonly IReportFormatter _formatter;
        private readonly ILogger _logger;
        private readonly IReportStorage _storage;

        public ReportsSaver(IReportFormatter formatter, IReportStorage storage, ILogger logger)
        {
            _formatter = formatter;
            _storage = storage;
            _logger = logger;
        }

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

        public void WriteReport(DayReport report)
        {
            _logger.WriteInfo($"Write day: {report.Day:dd-mm-yyyy}");
            IReportDataWriter writer = _storage.GetWriter(_formatter.GetName(report), WriterType.Day);

            try
            {
                writer.WriteHeader(_formatter.GetHeader(report));
                foreach (var country in report.AvailableCountries)
                {
                    _logger.WriteInfo($"Write day: {report.Day:dd-mm-yyyy} - {country}");
                    writer.WriteDataLine(_formatter.GetData(report, country));
                }
            }
            finally
            {
                writer.Close();
            }
        }

        private void WriteReport(BaseCountryReport report, string? parent)
        {
            IReportDataWriter writer = _storage.GetWriter(
                _formatter.GetName(report, parent),
                string.IsNullOrEmpty(parent) ? WriterType.Country : WriterType.Province);

            try
            {
                writer.WriteHeader(_formatter.GetHeader(report));
                foreach (var day in report.AvailableDates)
                {
                    writer.WriteDataLine(_formatter.GetData(report, day));
                }
            }
            finally
            {
                writer.Close();
            }
        }
    }
}