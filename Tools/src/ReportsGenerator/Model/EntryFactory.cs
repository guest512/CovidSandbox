using System;
using System.Collections.Generic;
using ReportsGenerator.Data;
using ReportsGenerator.Model.Processors;
using ReportsGenerator.Utils;

namespace ReportsGenerator.Model
{
    /// <summary>
    /// A factory class to create <see cref="Entry"/> from <see cref="Row"/> objects.
    /// </summary>
    public class EntryFactory
    {
        private readonly ILogger _logger;
        private readonly IDictionary<RowVersion, IRowProcessor> _rowProcessors;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntryFactory"/> class.
        /// </summary>
        /// <param name="rowProcessors">Collection of row processors <see cref="IRowProcessor"/> for supported <see cref="RowVersion"/> versions.</param>
        /// <param name="logger">A <see cref="ILogger"/> instance.</param>
        public EntryFactory(IDictionary<RowVersion, IRowProcessor> rowProcessors, ILogger logger)
        {
            _rowProcessors = rowProcessors;
            _logger = logger;
        }

        /// <summary>
        /// Creates <see cref="Entry"/> from the specified <see cref="Row"/>.
        /// </summary>
        /// <param name="row">An object to create <see cref="Entry"/>.</param>
        /// <returns>Initialized <see cref="Entry"/> object.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Throws, if <see cref="Row"/> has unsupported <see cref="RowVersion"/> version.</exception>
        public Entry CreateEntry(Row row)
        {
            if (_rowProcessors.ContainsKey(row.Version))
                return new Entry(row, _rowProcessors[row.Version]);

            _logger.WriteError($"row version '{row.Version}' is not supported");
            throw new ArgumentOutOfRangeException(nameof(row.Version), "Unknown row version");
        }
    }
}