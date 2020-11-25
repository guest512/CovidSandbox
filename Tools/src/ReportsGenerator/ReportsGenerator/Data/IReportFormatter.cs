using System.Collections.Generic;

namespace ReportsGenerator.Data
{
    /// <summary>
    /// Represents an output report formatter interface.
    /// Allows to get representation of report's data serialized to <typeparamref name="TResult"/>, its header, and name parts.
    /// </summary>
    /// <typeparam name="TResult">Report data type.</typeparam>
    public interface IReportFormatter<out TResult>
    {
        /// <summary>
        /// Gets data for the particular country in <see cref="IFormattableReport{TRow,TName}"/>.
        /// </summary>
        /// <typeparam name="TRow">Report row id type parameter.</typeparam>
        /// <typeparam name="TName">Report names type parameter.</typeparam>
        /// <param name="report">Report to read from.</param>
        /// <param name="row">Row to looking for.</param>
        /// <returns>A collection of data fields in the same order as in <see cref="GetHeader{TRow,TName}"/> collection, formatted and converted to <typeparamref name="TResult"/>.</returns>
        IEnumerable<TResult> GetData<TRow, TName>(IFormattableReport<TRow, TName> report, TRow row);

        /// <summary>
        /// Gets header for the particular report type.
        /// </summary>
        /// <typeparam name="TRow">Report row id type parameter.</typeparam>
        /// <typeparam name="TName">Report names type parameter.</typeparam>
        /// <param name="report">Report to read from.</param>
        /// <returns>A collection of properties converted to string.</returns>
        IEnumerable<string> GetHeader<TRow, TName>(IFormattableReport<TRow, TName> report);

        /// <summary>
        /// Get name parts, considering parent child relation, if applicable.
        /// </summary>
        /// <typeparam name="TRow">Report row id type parameter.</typeparam>
        /// <typeparam name="TName">Report names type parameter.</typeparam>
        /// <param name="report">Report to read from.</param>
        /// <returns>Name parts, that should be processed accordingly - joined together, ignored, replaced, etc.</returns>
        IEnumerable<string> GetName<TRow, TName>(IFormattableReport<TRow, TName> report);
    }
}