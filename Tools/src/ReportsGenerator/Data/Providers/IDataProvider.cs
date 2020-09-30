using System.Collections.Generic;

namespace ReportsGenerator.Data.Providers
{
    public interface IDataProvider
    {
        IEnumerable<Field> GetFields(RowVersion version);

        RowVersion GetVersion(string[] header);
    }
}