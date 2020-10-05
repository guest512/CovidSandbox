using System;
using System.Collections.Generic;

namespace ReportsGenerator.Data.IO
{
    public interface IReportDataWriter : IDisposable
    {
        void Close();

        void WriteDataLine(IEnumerable<string> data);

        void WriteHeader(IEnumerable<string> header);

        void IDisposable.Dispose()
        {
            Close();
        }
    }
}