namespace ReportsGenerator.Model
{
    public record Metrics(long Confirmed, long Active, long Recovered, long Deaths)
    {
        public static Metrics Empty { get; } = new Metrics(0, 0, 0, 0);

        public static Metrics FromEntry(Entry entry)
        {
            return entry != Entry.Empty ? new Metrics(entry.Confirmed,
                entry.Active,
                entry.Recovered,
                entry.Deaths) : Metrics.Empty;
        }

        public static Metrics operator -(Metrics left, Metrics right)
        {
            return new Metrics(left.Confirmed - right.Confirmed,
                left.Active - right.Active,
                left.Recovered - right.Recovered,
                left.Deaths - right.Deaths
            );
        }

        public static Metrics operator +(Metrics left, Metrics right)
        {
            return new Metrics(left.Confirmed + right.Confirmed,
                left.Active + right.Active,
                left.Recovered + right.Recovered,
                left.Deaths + right.Deaths
            );
        }
        public override string ToString() => $"{Confirmed} {Active} {Recovered} {Deaths}";
    }
}