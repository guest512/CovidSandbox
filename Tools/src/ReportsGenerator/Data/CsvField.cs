namespace ReportsGenerator.Data
{
    /// <summary>
    /// An abstraction for <see cref="Row"/> field. Inspired by CSV cell.
    /// </summary>
    public readonly struct CsvField
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CsvField"/> class.
        /// </summary>
        /// <param name="name">Field name.</param>
        /// <param name="value">Field value.</param>
        public CsvField(Field name, string value)
        {
            Name = name;
            Value = value;
        }

        /// <summary>
        /// Gets a field name.
        /// </summary>
        public Field Name { get; }

        /// <summary>
        /// Gets a field value.
        /// </summary>
        public string Value { get; }
    }
}