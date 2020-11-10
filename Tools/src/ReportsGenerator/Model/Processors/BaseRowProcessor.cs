using System;
using System.Diagnostics;
using ReportsGenerator.Data;
using ReportsGenerator.Utils;

namespace ReportsGenerator.Model.Processors
{
    public abstract class BaseRowProcessor : IRowProcessor
    {
        protected readonly ILogger Logger;
        private readonly IStatsProvider _statsProvider;

        protected BaseRowProcessor(IStatsProvider statsProvider, ILogger logger)
        {
            _statsProvider = statsProvider;
            Logger = logger;
        }

        public virtual long GetActive(Row row) => GetConfirmed(row) - GetDeaths(row) - GetRecovered(row);

        public virtual long GetConfirmed(Row row) => row[Field.Confirmed].AsLong();

        public abstract string GetCountryName(Row row);

        public abstract string GetCountyName(Row row);

        public virtual long GetDeaths(Row row) => row[Field.Deaths].AsLong();

        public IsoLevel GetIsoLevel(Row row)
        {
            var countyNameSet = !string.IsNullOrEmpty(GetCountyName(row));
            var provinceNameSet = !string.IsNullOrEmpty(GetProvinceName(row));
            var countryNameSet = !string.IsNullOrEmpty(GetCountryName(row));
            IsoLevel level;

            if (countryNameSet)
            {
                if (provinceNameSet)
                {
                    level = countyNameSet ? IsoLevel.County : IsoLevel.ProvinceState;
                }
                else
                {
                    if (countyNameSet)
                    {
                        Logger.WriteWarning("The row has country and county names but doesn't have province name. Treat it as country level.");
                        Debugger.Break();
                    }

                    level = IsoLevel.CountryRegion;
                }
            }
            else
            {
                Logger.WriteError("The row doesn't have a country name.");
                Debugger.Break();
                throw new InvalidOperationException("The row doesn't have a country name.");
            }

            return level;
        }

        public virtual DateTime GetLastUpdate(Row row) => row[Field.LastUpdate].AsDate();

        public abstract Origin GetOrigin(Row row);

        public abstract string GetProvinceName(Row row);

        public virtual long GetRecovered(Row row) => row[Field.Recovered].AsLong();

        public string GetStatsName(Row row) => _statsProvider.GetStatsName(row);
    }
}