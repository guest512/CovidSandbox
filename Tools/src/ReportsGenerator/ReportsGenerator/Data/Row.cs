using System.Collections.Generic;
using System.Linq;

namespace ReportsGenerator.Data
{
    /// <summary>
    /// Represents an abstraction for data row, inspired by .csv file row.
    /// </summary>
    public class Row
    {
        private readonly Dictionary<FieldId, string> _data;

        /// <summary>
        /// Initializes a new instance of the <see cref="Row"/> class.
        /// </summary>
        /// <param name="fields"><see cref="Field"/> collection to store in row.</param>
        /// <param name="version">Row version.</param>
        public Row(IEnumerable<Field> fields, RowVersion version)
        {
            Version = version;
            _data = fields.ToDictionary(_ => _.Id, _ => _.Value);
        }

        /// <summary>
        /// Gets a row version.
        /// </summary>
        public RowVersion Version { get; }

        /// <summary>
        /// Gets a value by provided key.
        /// </summary>
        /// <param name="key">A key to lookup in row.</param>
        /// <returns>A data value for the specified key, or empty string, if key not found.</returns>
        public string this[FieldId key] => _data.ContainsKey(key) ? _data[key] : string.Empty;
    }
}