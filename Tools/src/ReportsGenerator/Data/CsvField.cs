namespace ReportsGenerator.Data
{
    /// <summary>
    /// An abstraction for <see cref="Row"/> field. Inspired by CSV cell.
    /// </summary>
    public record CsvField(Field Name, string Value);
}