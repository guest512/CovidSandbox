using ReportsGenerator.Data.IO;

namespace ReportsGenerator.Data
{
    /// <summary>
    /// Represents storage report type enum.
    /// Helps <see cref="IReportStorage{TFormat}"/> determine which <see cref="IReportDataWriter{TFormat}"/> should be used.
    /// </summary>
    public enum ReportType
    {
        /// <summary>
        /// Country report type.
        /// </summary>
        Country,

        /// <summary>
        /// Day-by-day report type.
        /// </summary>
        Day,

        /// <summary>
        /// Statistical information report type.
        /// </summary>
        Stats,

        /// <summary>
        /// Internal model dump
        /// </summary>
        Model
    }
}