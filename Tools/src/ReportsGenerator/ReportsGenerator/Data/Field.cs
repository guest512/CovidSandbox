namespace ReportsGenerator.Data
{
    /// <summary>
    /// An abstraction for <see cref="ReportsGenerator.Data.Row"/> field. Inspired by CSV cell.
    /// </summary>
    public record Field(FieldId Id, string Value);
}