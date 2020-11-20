using System;
using System.Collections.Generic;
using ReportsGenerator.Model.Reports;

namespace ReportsGenerator.Data
{
    /// <summary>
    /// Represents an output report formatter interface.
    /// Allows to get representation of report's data serialized to string, its header, and name parts.
    /// </summary>
    public interface IReportFormatter
    {
        /// <summary>
        /// Gets data for the particular country in <see cref="DayReport"/>.
        /// </summary>
        /// <param name="report">Report to read from.</param>
        /// <param name="country">Country to looking for.</param>
        /// <returns>A collection of data fields in the same order as in <see cref="GetHeader{T}"/> collection, formatted and converted to string.</returns>
        IEnumerable<string> GetData(DayReport report, string country);

        /// <summary>
        /// Gets data for the particular day in <see cref="BaseCountryReport"/>.
        /// </summary>
        /// <param name="report">Report to read from.</param>
        /// <param name="day">Day to looking for.</param>
        /// <returns>A collection of data fields in the same order as in <see cref="GetHeader{T}"/> collection, formatted and converted to string.</returns>
        IEnumerable<string> GetData(BaseCountryReport report, DateTime day);

        /// <summary>
        /// Gets data for the <see cref="StatsReport"/>.
        /// </summary>
        /// <param name="report">Report to read from.</param>
        /// <returns>A collection of data fields in the same order as in <see cref="GetHeader{T}"/> collection, formatted and converted to string.</returns>
        IEnumerable<string> GetData(StatsReportNode report);

        /// <summary>
        /// Gets header for the particular report type.
        /// </summary>
        /// <typeparam name="TReportType">Report type. Could be one of the following: <see cref="DayReport"/>, <see cref="BaseCountryReport"/>, <see cref="StatsReport"/>.</typeparam>
        /// <returns></returns>
        IEnumerable<string> GetHeader<TReportType>();

        /// <summary>
        /// Get name parts, considering parent child relation, if applicable.
        /// </summary>
        /// <param name="report">Report to read from.</param>
        /// <returns>Name parts, that should be processed accordingly - joined together, ignored, replaced, etc.</returns>
        IEnumerable<string> GetName(DayReport report);

        /// <summary>
        /// Get name parts, considering parent child relation, if applicable.
        /// </summary>
        /// <param name="report">Report to read from.</param>
        /// <param name="parent">Parent for the report if exists. </param>
        /// <returns>Name parts, that should be processed accordingly - joined together, ignored, replaced, etc.</returns>
        public IEnumerable<string> GetName(BaseCountryReport report, string? parent = null);

        /// <summary>
        /// Get name parts, considering parent child relation, if applicable.
        /// </summary>
        /// <param name="report">Report to read from.</param>
        /// <returns>Name parts, that should be processed accordingly - joined together, ignored, replaced, etc.</returns>
        IEnumerable<string> GetName(StatsReportNode report);
    }
}