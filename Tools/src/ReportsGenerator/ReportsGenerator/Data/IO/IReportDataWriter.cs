using System;
using System.Collections.Generic;

namespace ReportsGenerator.Data.IO
{
    /// <summary>
    /// An interface for <see cref="IReportStorage{TFormat}"/> writer.
    /// </summary>
    public interface IReportDataWriter<in TFormat> : IDisposable
    {
        /// <summary>
        /// Signals to writer that all data is written so its resources could be released.
        /// </summary>
        void Close();

        /// <summary>
        /// Writes report data.
        /// </summary>
        /// <param name="data">Data to write.</param>
        void WriteDataLine(IEnumerable<TFormat> data);

        /// <summary>
        /// Writes header data.
        /// </summary>
        /// <param name="header">Header to write.</param>
        void WriteHeader(IEnumerable<string> header);

        void IDisposable.Dispose()
        {
            Close();
            GC.SuppressFinalize(this);
        }
    }
}