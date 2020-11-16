namespace ReportsGenerator.Model
{
    /// <summary>
    /// An <see cref="Entry"/> origin, based on <see cref="Data.Row.Version"/> value.
    /// </summary>
    public enum Origin
    {
        /// <summary>
        /// Entry is from JHopkins data source.
        /// </summary>
        JHopkins,

        /// <summary>
        /// Entry is from Yandex data source.
        /// </summary>
        Yandex
    }
}