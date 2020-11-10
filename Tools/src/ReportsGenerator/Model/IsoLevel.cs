namespace ReportsGenerator.Model
{
    /// <summary>
    /// Represents an object ISO level (Country - 1, Province - 2, County - 3).
    /// </summary>
    public enum IsoLevel
    {
        /// <summary>
        /// Country level.
        /// </summary>
        CountryRegion = 1,

        /// <summary>
        /// Province level.
        /// </summary>
        ProvinceState = 2,

        /// <summary>
        /// County level.
        /// </summary>
        County = 3
    }
}