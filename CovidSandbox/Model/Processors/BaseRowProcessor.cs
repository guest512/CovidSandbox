using System;
using CovidSandbox.Data;

namespace CovidSandbox.Model.Processors
{
    public abstract class BaseRowProcessor : IRowProcessor
    {
        protected const string MainCountryRegion = "Main territory";

        public virtual long GetActive(Row row)
        {
            var active = TryGetValue(row[Field.Active], long.MinValue);
            return active == long.MinValue ? GetConfirmed(row) - GetDeaths(row) - GetRecovered(row) : active;
        }

        public virtual long GetConfirmed(Row row) => TryGetValue(row[Field.Confirmed]);

        public abstract string GetCountryName(Row row);

        public abstract string GetCountyName(Row row);

        public virtual long GetDeaths(Row row) => TryGetValue(row[Field.Deaths]);

        public abstract uint GetFips(Row row);

        public abstract Origin GetOrigin(Row row);

        public abstract string GetProvinceName(Row row);

        public virtual long GetRecovered(Row row) => TryGetValue(row[Field.Recovered]);

        public virtual DateTime GetLastUpdate(Row row) => Data.Utils.ParseDate(row[Field.LastUpdate]);

        protected static long TryGetValue(string stringValue, long defaultValue = 0) =>
            long.TryParse(stringValue, out var intValue) ? intValue : defaultValue;
    }
}