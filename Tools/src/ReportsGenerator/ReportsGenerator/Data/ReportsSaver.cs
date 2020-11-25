using System;
using ReportsGenerator.Data.IO;
using ReportsGenerator.Utils;

namespace ReportsGenerator.Data
{
    /// <summary>
    /// Represents a helper class to save reports using particular <see cref="IReportFormatter{TResult}"/> and <see cref="IReportStorage{TFormat}"/>.
    /// </summary>
    /// <typeparam name="TFormat">Formatter and storage data type.</typeparam>
    public class ReportsSaver<TFormat> : IDisposable
    {
        private readonly IReportFormatter<TFormat> _formatter;
        private readonly ILogger _logger;
        private readonly IReportStorage<TFormat> _storage;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportsSaver{TFormat}"/> class.
        /// </summary>
        /// <param name="formatter">Desired report formatter to read reports.</param>
        /// <param name="storage">Desired storage to save reports data.</param>
        /// <param name="logger">A <see cref="ILogger"/> instance.</param>
        public ReportsSaver(IReportFormatter<TFormat> formatter, IReportStorage<TFormat> storage, ILogger logger)
        {
            _formatter = formatter;
            _storage = storage;
            _logger = logger;
        }

        /// <summary>
        /// Writes <see cref="IFormattableReport{TRow,TName}"/>.
        /// </summary>
        /// <typeparam name="TRow">Report row id type parameter.</typeparam>
        /// <typeparam name="TName">Report names type parameter.</typeparam>
        /// <param name="report">Report to write.</param>
        public void WriteReport<TRow, TName>(IFormattableReport<TRow, TName> report)
        {
            using var writer = _storage.GetWriter(_formatter.GetName(report), report.ReportType);

            writer.WriteHeader(_formatter.GetHeader(report));
            foreach (var rowId in report.RowIds)
            {
                writer.WriteDataLine(_formatter.GetData(report, rowId));
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _storage.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}