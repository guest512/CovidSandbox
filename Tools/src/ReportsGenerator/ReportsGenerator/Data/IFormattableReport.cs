using System.Collections;
using System.Collections.Generic;
using ReportsGenerator.Data.IO;

namespace ReportsGenerator.Data
{
    /// <summary>
    /// Interface to serialize report to write it in <see cref="IReportStorage{TFormat}"/> using corresponding <see cref="IReportFormatter{TResult}"/>.
    /// </summary>
    /// <typeparam name="TRow">Report row id type parameter.</typeparam>
    /// <typeparam name="TName">Report names type parameter.</typeparam>
    public interface IFormattableReport<TRow, out TName>
    {
        /// <summary>
        /// Gets report name parts, considering parent child relation, if applicable.
        /// </summary>
        public IEnumerable<TName> Name { get; }

        /// <summary>
        /// Gets report properties names.
        /// </summary>
        public IEnumerable<string> Properties { get; }

        /// <summary>
        /// Gets reports row indexes, to use them in <see cref="GetValue"/> function.
        /// </summary>
        public IEnumerable<TRow> RowIds { get; }

        /// <summary>
        /// Gets report type, to help <see cref="IReportStorage{TFormat}"/> treat report correctly.
        /// </summary>
        public ReportType ReportType { get; }

        /// <summary>
        /// Gets <paramref name="property"/> value for the specified row (<paramref name="key"/>).
        /// </summary>
        /// <param name="property">Property to search value for.</param>
        /// <param name="key">Row identifier to search value.</param>
        /// <returns>Property value.</returns>
        public object GetValue(string property, TRow key);
    }
}