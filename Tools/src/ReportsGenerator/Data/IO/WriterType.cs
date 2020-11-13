namespace ReportsGenerator.Data.IO
{
    /// <summary>
    /// Supported writer types.
    /// </summary>
    public enum WriterType
    {
        /// <summary>
        /// Used for <see cref="ReportsGenerator.Model.Reports.DayReport"/>.
        /// </summary>
        Day,

        /// <summary>
        /// Used for <see cref="ReportsGenerator.Model.Reports.BaseCountryReport"/>.
        /// </summary>
        Country,

        /// <summary>
        /// Used for <see cref="ReportsGenerator.Model.Reports.StatsReport"/>.
        /// </summary>
        Stats
    }
}