using System;
using System.Collections.Generic;
using ReportsGenerator.Data;
using ReportsGenerator.Model.Processors;
using ReportsGenerator.Utils;

namespace ReportsGenerator.Model
{
    public class EntryFactory
    {
        private readonly ILogger _logger;
        private readonly IDictionary<RowVersion, IRowProcessor> _rowProcessors;

        public EntryFactory(IDictionary<RowVersion, IRowProcessor> rowProcessors, ILogger logger)
        {
            _rowProcessors = rowProcessors;
            _logger = logger;
        }

        public Entry CreateEntry(Row row)
        {
            if (_rowProcessors.ContainsKey(row.Version))
            {
                var rowProcessor = _rowProcessors[row.Version];
                return new Entry
                {
                    ProvinceState = rowProcessor.GetProvinceName(row),
                    CountryRegion = rowProcessor.GetCountryName(row),
                    LastUpdate = rowProcessor.GetLastUpdate(row),
                    Confirmed = rowProcessor.GetConfirmed(row),
                    Deaths = rowProcessor.GetDeaths(row),
                    Recovered = rowProcessor.GetRecovered(row),
                    Active = rowProcessor.GetActive(row),
                    County = rowProcessor.GetCountyName(row),
                    Origin = rowProcessor.GetOrigin(row),
                    IsoLevel = rowProcessor.GetIsoLevel(row),
                    StatsName = rowProcessor.GetStatsName(row)
                };
            }

            _logger.WriteError($"row version '{row.Version}' is not supported");
            throw new ArgumentOutOfRangeException(nameof(row.Version), "Unknown row version");
        }
    }
}