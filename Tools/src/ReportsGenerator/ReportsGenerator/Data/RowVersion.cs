namespace ReportsGenerator.Data
{
    /// <summary>
    /// Represents a data version for the <see cref="Row"/>.
    /// </summary>
    public enum RowVersion
    {
        /// <summary>
        /// JHopkins data source file used from 22-01-2020 till 29-02-2020.
        /// </summary>
        JHopkinsV1,

        /// <summary>
        /// JHopkins data source file used from 01-03-2020 till 21-03-2020.
        /// </summary>
        JHopkinsV2,

        /// <summary>
        /// JHopkins data source file used from 22-03-2020 till 28-05-2020.
        /// </summary>
        JHopkinsV3,

        /// <summary>
        /// JHopkins data source file used from 29-05-2020 till 08-11-2020.
        /// </summary>
        JHopkinsV4,

        /// <summary>
        /// JHopkins data source file used from 09-11-2020 till now.
        /// </summary>
        JHopkinsV5,

        /// <summary>
        /// Yandex data source file.
        /// </summary>
        YandexRussia,

        /// <summary>
        /// JHopkins UID_ISO_FIPS_LookUp_Table file.
        /// </summary>
        StatsBase,

        /// <summary>
        /// Additional statistics information file.
        /// </summary>
        StatsEx,

        /// <summary>
        /// Cyrillic-Latin translation file.
        /// </summary>
        Translation,

        /// <summary>
        /// Abbreviation-FullName information file.
        /// </summary>
        State,

        /// <summary>
        /// Not supported.
        /// </summary>
        Unknown
    }
}