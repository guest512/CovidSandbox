using System.Collections.Generic;

namespace ReportsGenerator.Data.DataSources
{
    /// <summary>
    /// A function definition used by <see cref="IDataSourceReader.GetRowsAsync{T}"/> as a callback.
    /// </summary>
    /// <typeparam name="TResult">Returned result type.</typeparam>
    /// <param name="row"><see cref="Row"/> to convert.</param>
    /// <returns>An instance of the result type.</returns>
    public delegate TResult GetRowsCallback<out TResult>(Row row);

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
        /// <param name="callback">A function to call to convert <see cref="Row"/> to instance of TResult.</param>
        /// <returns>TResult objects collection.</returns>
        IAsyncEnumerable<TResult> GetRowsAsync<TResult>(GetRowsCallback<TResult> callback);
    }
}