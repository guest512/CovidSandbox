using System;
using System.Diagnostics;
using ReportsGenerator.Data;
using ReportsGenerator.Utils;

namespace ReportsGenerator.Model.Processors
{
    /// <summary>
    /// Represents a common implementation of the <see cref="IRowProcessor"/> interface. Cannot be instantiated.
    /// </summary>
    public abstract class BaseRowProcessor : IRowProcessor
    {
        /// <summary>
        /// A <see cref="ILogger"/> instance.
        /// </summary>
        private readonly ILogger _logger;

        private readonly IStatsProvider _statsProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseRowProcessor"/> class.
        /// </summary>
        /// <param name="statsProvider">Statistical provider to generate key.</param>
        /// <param name="logger">A <see cref="ILogger"/> instance.</param>
        protected BaseRowProcessor(IStatsProvider statsProvider, ILogger logger)
        {
            _statsProvider = statsProvider;
            _logger = logger;
        }

        /// <inheritdoc />
        public virtual long GetActive(Row row) => GetConfirmed(row) - GetDeaths(row) - GetRecovered(row);

        /// <inheritdoc />
        public virtual long GetConfirmed(Row row) => row[FieldId.Confirmed].AsLong();

        /// <inheritdoc />
        public abstract string GetCountryName(Row row);

        /// <inheritdoc />
        public abstract string GetCountyName(Row row);

        /// <inheritdoc />
        public virtual long GetDeaths(Row row) => row[FieldId.Deaths].AsLong();

        /// <inheritdoc />
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
                        _logger.WriteWarning("The row has country and county names but doesn't have province name. Treat it as country level.");
                        Debugger.Break();
                    }

                    level = IsoLevel.CountryRegion;
                }
            }
            else
            {
                _logger.WriteError("The row doesn't have a country name.");
                Debugger.Break();
                throw new InvalidOperationException("The row doesn't have a country name.");
            }

            return level;
        }

        /// <inheritdoc />
        public virtual DateTime GetLastUpdate(Row row) => row[FieldId.LastUpdate].AsDate();

        /// <inheritdoc />
        public abstract Origin GetOrigin(Row row);

        /// <inheritdoc />
        public abstract string GetProvinceName(Row row);

        /// <inheritdoc />
        public virtual long GetRecovered(Row row) => row[FieldId.Recovered].AsLong();

        /// <inheritdoc />
        public string GetStatsName(Row row) => _statsProvider.GetStatsName(row);
    }
}