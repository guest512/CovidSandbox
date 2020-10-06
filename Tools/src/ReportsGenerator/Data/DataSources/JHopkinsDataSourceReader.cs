using System.Collections.Generic;
using System.IO;
using ReportsGenerator.Data.DataSources.Providers;
using ReportsGenerator.Utils;

namespace ReportsGenerator.Data.DataSources
{
    public class JHopkinsDataSourceReader : CsvFilesDataSourceReader
    {
        public JHopkinsDataSourceReader(IEnumerable<string> files, IDictionary<RowVersion, IDataProvider> dataProviders, ILogger logger): base(files, dataProviders, logger)
        {
        }


        protected override CsvField CsvFieldCreator(Field key, string value, string fileName)
        {
            return key == Field.LastUpdate
                ? new CsvField(key, Path.GetFileNameWithoutExtension(fileName))
                : new CsvField(key, value);
        }
    }
}