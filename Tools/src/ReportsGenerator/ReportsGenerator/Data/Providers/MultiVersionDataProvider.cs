using System.Collections.Generic;
using System.Linq;

namespace ReportsGenerator.Data.Providers
{
    /// <summary>
    /// An implementation of <see cref="IDataProvider"/> for cases when one provider supports more than one <see cref="RowVersion"/>.
    /// </summary>
    public abstract class MultiVersionDataProvider : IDataProvider
    {
        /// <summary>
        /// Dictionary to store <see cref="ICollection{T}"/> of <see cref="FieldId"/> for each supported <see cref="RowVersion"/>.
        /// </summary>
        protected readonly IDictionary<RowVersion, ICollection<FieldId>> VersionFieldsDictionary;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiVersionDataProvider"/> class.
        /// </summary>
        protected MultiVersionDataProvider()
        {
            VersionFieldsDictionary = new Dictionary<RowVersion, ICollection<FieldId>>();
        }

        /// <inheritdoc />
        public IEnumerable<FieldId> GetFields(RowVersion version)
        {
            return VersionFieldsDictionary.ContainsKey(version) ? VersionFieldsDictionary[version] : Enumerable.Empty<FieldId>();
        }

        /// <inheritdoc />
        public RowVersion GetVersion(ICollection<string> header)
        {
            var headersCount = header.Count;

            var (rowVersion, fields) = VersionFieldsDictionary
                .FirstOrDefault(_ => headersCount == _.Value.Count() && ValidateColumnsOrder(_.Key, header));

            return fields == null ? RowVersion.Unknown : rowVersion;
        }

        /// <summary>
        /// Helper function that returns a <see cref="string"/> representation of given <see cref="FieldId"/> in the particular <see cref="RowVersion"/>.
        /// </summary>
        /// <param name="field">Field to convert to string.</param>
        /// <param name="version"><see cref="RowVersion"/> to where this field come from.</param>
        /// <returns>A <see cref="string"/> representation of given <see cref="FieldId"/>.</returns>
        protected abstract string FieldToString(FieldId field, RowVersion version);

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