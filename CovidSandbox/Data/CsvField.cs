namespace CovidSandbox.Data
{
    public readonly struct CsvField
    {
        public CsvField(Field name, string value)
        {
            Name = name;
            Value = value;
        }

        public Field Name { get; }
        public string Value { get; }
    }
}