using System.Collections.Generic;

namespace ReportsGenerator.Data.DataSources.Providers
{
    public interface IDataProvider
    {
        IEnumerable<Field> GetFields(RowVersion version);

        RowVersion GetVersion(IEnumerable<string> header);
    }
}