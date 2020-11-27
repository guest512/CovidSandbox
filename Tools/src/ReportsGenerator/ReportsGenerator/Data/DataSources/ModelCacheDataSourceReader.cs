using System.Collections.Generic;
using ReportsGenerator.Data.Providers;
using ReportsGenerator.Utils;

namespace ReportsGenerator.Data.DataSources
{
    /// <summary>
    /// Represents a <see cref="ModelCacheDataSourceReader"/> for cached data model.
    /// </summary>
    public class ModelCacheDataSourceReader: CsvFilesDataSourceReader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModelCacheDataSourceReader"/> class.
        /// </summary>
        /// <inheritdoc/>
        public ModelCacheDataSourceReader(IEnumerable<string> files, IDataProvider dataProvider, ILogger logger) : base(files, dataProvider, logger)
        {
        }
    }
}