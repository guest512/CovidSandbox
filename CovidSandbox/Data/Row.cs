using System.Collections.Generic;
using System.Linq;

namespace CovidSandbox.Data
{
    public class Row
    {
        private readonly Dictionary<Field, string> _data;

        public Row(IEnumerable<CsvField> fields, RowVersion version)
        {
            Version = version;
            _data = fields.ToDictionary(_ => _.Name, _ => _.Value);
        }

        public RowVersion Version { get; }
        public string this[Field key] => _data.ContainsKey(key) ? _data[key] : string.Empty;
    }
}