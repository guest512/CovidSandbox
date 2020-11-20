using System.Collections.Generic;

namespace ReportsGenerator.Data.IO
{
    /// <summary>
    /// An abstraction for storage where reports should be written.
    /// </summary>
    public interface IReportStorage
    {
        /// <summary>
        /// Returns a supported <see cref="IReportDataWriter"/> object for the storage.
        /// </summary>
        /// <param name="name">Report name parts that could be retrieved from <see cref="IReportFormatter"/>.</param>
        /// <param name="reportType">A <see cref="WriterType"/>.</param>
        /// <returns>A supported data writer.</returns>
        IReportDataWriter GetWriter(IEnumerable<string> name, WriterType reportType);
    }
}