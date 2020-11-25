using System;
using System.Collections.Generic;

namespace ReportsGenerator.Data.IO
{
    /// <summary>
    /// An abstraction for storage where reports should be written.
    /// </summary>
    /// <typeparam name="TFormat"></typeparam>
    public interface IReportStorage<in TFormat> : IDisposable
    {
        /// <summary>
        /// Returns a supported <see cref="IReportDataWriter{TFormat}"/> object for the storage.
        /// </summary>
        /// <param name="name">Report name parts that could be retrieved from <see cref="IReportFormatter{TResult}"/>.</param>
        /// <param name="type">A <see cref="ReportType"/>.</param>
        /// <returns>A supported data writer.</returns>
        IReportDataWriter<TFormat> GetWriter(IEnumerable<string> name, ReportType type);

        /// <summary>
        /// Closes storage and all its writers.
        /// </summary>
        void Close();

        void IDisposable.Dispose()
        {
            Close();
            GC.SuppressFinalize(this);
        }
    }
}