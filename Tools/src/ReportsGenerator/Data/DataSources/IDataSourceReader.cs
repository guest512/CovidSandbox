using System;
using System.Collections.Generic;

namespace ReportsGenerator.Data.DataSources
{
    public interface IDataSourceReader
    {
        IEnumerable<Row> GetRows();

        IAsyncEnumerable<Row> GetRowsAsync();

        IAsyncEnumerable<T> GetRowsAsync<T>(Func<Row, T> callback);
    }
}