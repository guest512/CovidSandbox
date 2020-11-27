using System.Collections.Generic;

namespace ReportsGenerator.Data.DataSources
{
    /// <summary>
    /// A function definition used by <see cref="IDataSourceReader"/> functions as a callback to convert <see cref="Row"/> to <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TResult">Returned result type.</typeparam>
    /// <param name="row"><see cref="Row"/> to convert.</param>
    /// <returns>An instance of the result type.</returns>
    public delegate TResult GetRowsCallback<out TResult>(Row row);

    /// <summary>
    /// A function definition  used by <see cref="IDataSourceReader"/> functions to skip <see cref="Row"/> by condition.
    /// </summary>
    /// <param name="field">Field to determine whether or not row(-s) should be skipped.</param>
    /// <returns><see langword="false"/> if row should be skipped, otherwise returns <see langword="false"/>.</returns>
    public delegate bool RowsFilter(Field field);

    /// <summary>
    /// An interface to read <see cref="IDataSource"/>.
    /// </summary>
    public interface IDataSourceReader
    {
        /// <summary>
        /// Reads <see cref="IDataSource"/> and returns its content as an <see cref="IEnumerable{T}"/> of <see cref="Row"/>.
        /// </summary>
        /// <returns>Rows collection.</returns>
        IEnumerable<Row> GetRows();

        /// <summary>
        /// Reads <see cref="IDataSource"/> and returns its content as an <see cref="IAsyncEnumerable{T}"/> of <see cref="Row"/>.
        /// </summary>
        /// <returns>Rows collection.</returns>
        IAsyncEnumerable<Row> GetRowsAsync();

        /// <summary>
        /// Reads <see cref="IDataSource"/> and returns its content as an <see cref="IAsyncEnumerable{TResult}"/> of TResult.
        /// </summary>
        /// <typeparam name="TResult">Returned result type.</typeparam>
        /// <param name="callback">A function to call to convert <see cref="Row"/> to instance of <typeparamref name="TResult"/>.</param>
        /// <returns><typeparamref name="TResult"/> objects collection.</returns>
        IAsyncEnumerable<TResult> GetRowsAsync<TResult>(GetRowsCallback<TResult> callback);

        /// <summary>
        /// Gets a collection of <see cref="FieldId"/> that could be used in <see cref="RowsFilter"/> calls.
        /// </summary>
        IEnumerable<FieldId> SupportedFilters { get; }

        /// <summary>
        /// Reads <see cref="IDataSource"/> and returns its content as an <see cref="IEnumerable{T}"/> of <see cref="Row"/>.
        /// </summary>
        /// <param name="filter">A function to call to determine whether or not row(-s) should be skipped.</param>
        /// <returns>Rows collection.</returns>
        IEnumerable<Row> GetRows(RowsFilter filter);

        /// <summary>
        /// Reads <see cref="IDataSource"/> and returns its content as an <see cref="IAsyncEnumerable{T}"/> of <see cref="Row"/>.
        /// </summary>
        /// <param name="filter">A function to call to determine whether or not row(-s) should be skipped.</param>
        /// <returns>Rows collection.</returns>
        IAsyncEnumerable<Row> GetRowsAsync(RowsFilter filter);

        /// <summary>
        /// Reads <see cref="IDataSource"/> and returns its content as an <see cref="IAsyncEnumerable{TResult}"/> of <typeparamref name="TResult"/>.
        /// </summary>
        /// <typeparam name="TResult">Returned result type.</typeparam>
        /// <param name="filter">A function to call to determine whether or not row(-s) should be skipped.</param>
        /// <param name="callback">A function to call to convert <see cref="Row"/> to instance of <typeparamref name="TResult"/>.</param>
        /// <returns><typeparamref name="TResult"/> objects collection.</returns>
        IAsyncEnumerable<TResult> GetRowsAsync<TResult>(RowsFilter filter, GetRowsCallback<TResult> callback);


    }
}