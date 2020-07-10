using CovidSandbox.Data;

namespace CovidSandbox.Model.Processors
{
    public abstract class BaseRowProcessor : IRowProcessor
    {
        protected const string MainCountryRegion = "Main territory";

        public virtual long GetActive(Row row) => TryGetValue(row[Field.Active]);

        public virtual long GetConfirmed(Row row) => TryGetValue(row[Field.Confirmed]);

        public abstract string GetCountryName(Row row);

        public abstract string GetCountyName(Row row);

        public virtual long GetDeaths(Row row) => TryGetValue(row[Field.Deaths]);

        public abstract uint GetFips(Row row);

        public abstract Origin GetOrigin(Row row);

        public abstract string GetProvinceName(Row row);

        public virtual long GetRecovered(Row row) => TryGetValue(row[Field.Recovered]);

        protected static long TryGetValue(string stringValue) =>
            long.TryParse(stringValue, out var intValue) ? intValue : 0;
    }
}