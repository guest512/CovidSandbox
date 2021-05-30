namespace ReportsGenerator.Model.Reports.Intermediate
{
    /// <summary>
    /// Represents an abstraction for intermediate geographical object.
    /// </summary>
    public record BasicMetadataReport(
        string Country = "",
        string Province = "",
        string County = "",
        string StatsName = "");
}