using ReportsGenerator.Data;

namespace ReportsGenerator.Model
{
    /// <summary>
    /// Represents a statistical information provider interface.
    /// </summary>
    public interface IStatsProvider
    {
        /// <summary>
        /// Gets a key to lookup for statistical information.
        /// </summary>
        /// <param name="row">A <see cref="Row"/> instance to generate a key.</param>
        /// <returns>A <see cref="string"/> representing a key to use in the Lookup... methods.</returns>
        string GetStatsName(Row row);

        /// <summary>
        /// Gets a key to lookup for country statistical information.
        /// </summary>
        /// <param name="countryName">A country name which key is needed.</param>
        /// <returns>A <see cref="string"/> representing a key to use in the Lookup... methods.</returns>
        string GetCountryStatsName(string countryName);

        /// <summary>
        /// Returns a continent name where an object is located.
        /// </summary>
        /// <param name="statsName">A key to lookup.</param>
        /// <returns>Continent name.</returns>
        string LookupContinentName(string statsName);

        /// <summary>
        /// Returns a population of an object.
        /// </summary>
        /// <param name="statsName">A key to lookup.</param>
        /// <returns>Population count.</returns>
        long LookupPopulation(string statsName);
    }
}