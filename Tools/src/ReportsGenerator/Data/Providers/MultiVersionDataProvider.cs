using System.Collections.Generic;
using System.Linq;

namespace ReportsGenerator.Data.Providers
{
    public abstract class MultiVersionDataProvider : IDataProvider
    {
        protected readonly IDictionary<RowVersion, ICollection<Field>> VersionFieldsDictionary;

        protected MultiVersionDataProvider()
        {
            VersionFieldsDictionary = new Dictionary<RowVersion, ICollection<Field>>();
        }

        public IEnumerable<Field> GetFields(RowVersion version)
        {
            return VersionFieldsDictionary.ContainsKey(version) ? VersionFieldsDictionary[version] : Enumerable.Empty<Field>();
        }

        public RowVersion GetVersion(ICollection<string> header)
        {
            var headersCount = header.Count;

            var (rowVersion, fields) = VersionFieldsDictionary
                .FirstOrDefault(_ => headersCount == _.Value.Count() && ValidateColumnsOrder(_.Key, header));

            return fields == null ? RowVersion.Unknown : rowVersion;
        }

        protected abstract string FieldToString(Field field, RowVersion version);

        private bool ValidateColumnsOrder(RowVersion version, IEnumerable<string> headerColumns)
        {
            using var headerEnumerator = headerColumns.GetEnumerator();
            using var fieldsEnumerator = VersionFieldsDictionary[version].GetEnumerator();

            while (headerEnumerator.MoveNext() && fieldsEnumerator.MoveNext())
            {
                if (headerEnumerator.Current != FieldToString(fieldsEnumerator.Current, version))
                    return false;
            }

            return true;
        }
    }
}