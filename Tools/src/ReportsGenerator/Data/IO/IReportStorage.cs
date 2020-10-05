using System.Collections.Generic;

namespace ReportsGenerator.Data.IO
{
    public interface IReportStorage
    {
        IReportDataWriter GetWriter(IEnumerable<string> name, WriterType reportType);
    }
}