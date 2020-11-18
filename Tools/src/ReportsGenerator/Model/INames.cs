namespace ReportsGenerator.Model
{
    /// <summary>
    /// Represents a helper service to work with geographical objects names.
    /// </summary>
    public interface INames
    {
        /// <summary>
        /// Translates object's Latin name to Cyrillic name.
        /// </summary>
        /// <param name="latinName">Name to translate.</param>
        /// <returns>Translated name.</returns>
        string GetCyrillicName(string latinName);

        /// <summary>
        /// Translates object's Cyrillic name to Latin name.
        /// </summary>
        /// <param name="cyrillicName">Name to translate.</param>
        /// <returns>Translated name.</returns>
        string GetLatinName(string cyrillicName);

        /// <summary>
        /// Translates state abbreviation to its full name.
        /// </summary>
        /// <param name="stateAbbrev">State abbreviation to translate.</param>
        /// <returns>State full name.</returns>
        string GetStateFullName(string stateAbbrev);
    }
}