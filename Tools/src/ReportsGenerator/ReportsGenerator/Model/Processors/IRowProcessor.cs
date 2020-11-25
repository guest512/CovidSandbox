using System;
using ReportsGenerator.Data;

namespace ReportsGenerator.Model.Processors
{
    /// <summary>
    /// Represents a row processor interface.
    /// Allows to retrieve data from <see cref="Row"/> in strong-typed manner or with additional processing if needed.
    /// </summary>
    public interface IRowProcessor
    {
        /// <summary>
        /// Returns number of active cases.
        /// </summary>
        /// <param name="row">Data source.</param>
        /// <returns>Number of cases.</returns>
        long GetActive(Row row);

        /// <summary>
        /// Returns number of confirmed cases.
        /// </summary>
        /// <param name="row">Data source.</param>
        /// <returns>Number of cases.</returns>
        long GetConfirmed(Row row);

        /// <summary>
        /// Returns country name.
        /// </summary>
        /// <param name="row">Data source.</param>
        /// <returns>Country name.</returns>
        string GetCountryName(Row row);

        /// <summary>
        /// Returns county name.
        /// </summary>
        /// <param name="row">Data source.</param>
        /// <returns>County name.</returns>
        string GetCountyName(Row row);

        /// <summary>
        /// Returns number of death cases.
        /// </summary>
        /// <param name="row">Data source.</param>
        /// <returns>Number of cases.</returns>
        long GetDeaths(Row row);

        /// <summary>
        /// Returns row's ISO level.
        /// </summary>
        /// <param name="row">Data source.</param>
        /// <returns><see cref="IsoLevel"/> value.</returns>
        IsoLevel GetIsoLevel(Row row);

        /// <summary>
        /// Returns date for which the row has data.
        /// </summary>
        /// <param name="row">Data source.</param>
        /// <returns>Parsed data.</returns>
        DateTime GetLastUpdate(Row row);

        /// <summary>
        /// Returns row's origin.
        /// </summary>
        /// <param name="row">Data source.</param>
        /// <returns>Number of cases.</returns>
        Origin GetOrigin(Row row);

        /// <summary>
        /// Returns province name.
        /// </summary>
        /// <param name="row">Data source.</param>
        /// <returns>Province name.</returns>
        string GetProvinceName(Row row);

        /// <summary>
        /// Returns number of recovered cases.
        /// </summary>
        /// <param name="row">Data source.</param>
        /// <returns>Number of cases.</returns>
        long GetRecovered(Row row);

        /// <summary>
        /// Returns key to lookup for statistical information.
        /// </summary>
        /// <param name="row">Data source.</param>
        /// <returns>A key to lookup.</returns>
        string GetStatsName(Row row);
    }
}