using System.Collections.Generic;

namespace CovidSandbox.Data.Providers
{
    public interface IDataProvider
    {
        IEnumerable<Field> GetFields(RowVersion version);

        RowVersion GetVersion(string[] header);
    }
}