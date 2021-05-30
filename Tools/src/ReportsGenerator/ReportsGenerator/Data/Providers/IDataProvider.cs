using System.Collections.Generic;

namespace ReportsGenerator.Data.Providers
{
    /// <summary>
    /// A data provider interface that allows to determine how to parse provided data row.
    /// </summary>
    public interface IDataProvider
    {
        /// <summary>
        /// Returns a <see cref="FieldId"/> collection for the particular <see cref="RowVersion"/>.
        /// </summary>
        /// <param name="version">Row version.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="FieldId"/>.</returns>
        IEnumerable<FieldId> GetFields(RowVersion version);

        /// <summary>
        /// Gets a <see cref="RowVersion"/> for the particular header.
        /// </summary>
        /// <param name="header">A data header in form of <see cref="ICollection{T}"/> of <see cref="string"/>.</param>
        /// <returns><see cref="RowVersion"/> of the header. If header is not supported or not known, then returns <see cref="RowVersion.Unknown"/>.</returns>
        RowVersion GetVersion(ICollection<string> header);
    }
}