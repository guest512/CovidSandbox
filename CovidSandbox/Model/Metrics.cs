using System;

namespace CovidSandbox.Model
{
    public readonly struct Metrics
    {
        public Metrics(in long confirmed, in long active, in long recovered, in long deaths)
        {
            Confirmed = confirmed;
            Deaths = deaths;
            Active = active;
            Recovered = recovered;
        }

        // ReSharper disable once UnassignedGetOnlyAutoProperty
        public static Metrics Empty { get; }

        public long Active { get; }

        public long Confirmed { get; }

        public long Deaths { get; }

        public long Recovered { get; }

        public static Metrics FromEntry(Entry entry)
        {
            return entry != null ? new Metrics(entry.Confirmed.GetValueOrDefault(),
                entry.Active.GetValueOrDefault(),
                entry.Recovered.GetValueOrDefault(),
                entry.Deaths.GetValueOrDefault()) : Metrics.Empty;
        }

        public static Metrics operator -(Metrics left, Metrics right)
        {
            return new Metrics(left.Confirmed - right.Confirmed,
                left.Active - right.Active,
                left.Recovered - right.Recovered,
                left.Deaths - right.Deaths
            );
        }

        public static bool operator !=(Metrics left, Metrics right) => !left.Equals(right);

        public static Metrics operator +(Metrics left, Metrics right)
        {
            return new Metrics(left.Confirmed + right.Confirmed,
                left.Active + right.Active,
                left.Recovered + right.Recovered,
                left.Deaths + right.Deaths
            );
        }

        public static bool operator ==(Metrics left, Metrics right) => left.Equals(right);

        public void Deconstruct(out long confirmed, out long active, out long recovered, out long deaths)
        {
            confirmed = Confirmed;
            active = Active;
            recovered = Recovered;
            deaths = Deaths;
        }

        public override bool Equals(object obj)
        {
            return obj is Metrics other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Confirmed, Deaths, Active, Recovered);
        }

        private bool Equals(Metrics other)
        {
            return Confirmed == other.Confirmed && Deaths == other.Deaths && Active == other.Active && Recovered == other.Recovered;
        }
    }
}