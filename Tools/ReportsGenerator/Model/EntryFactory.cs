using CovidSandbox.Data;
using CovidSandbox.Model.Processors;
using CovidSandbox.Utils;
using System;
using System.Collections.Generic;

namespace CovidSandbox.Model
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
                return new Entry(row, _rowProcessors[row.Version]);

            _logger.WriteError($"row version '{row.Version}' is not supported");
            throw new ArgumentOutOfRangeException(nameof(row.Version), "Unknown row version");
        }
    }
}