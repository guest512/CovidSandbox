using System;
using System.Collections.Generic;
using System.Text;

namespace CovidSandbox.Model
{
    public readonly struct Metrics
    {
        public static Metrics FromEntry (Entry entry)
        {
            return entry != null ? new Metrics(entry.Confirmed.GetValueOrDefault(),
                entry.Active.GetValueOrDefault(),
                entry.Recovered.GetValueOrDefault(),
                entry.Deaths.GetValueOrDefault()) : Metrics.Empty;
        }
        public Metrics(in long confirmed, in long active, in long recovered, in long deaths)
        {
            Confirmed = confirmed;
            Deaths = deaths;
            Active = active;
            Recovered = recovered;
        }

        public void Deconstruct(out long confirmed, out long active, out long recovered, out long deaths)
        {
            confirmed = Confirmed;
            active = Active;
            recovered = Recovered;
            deaths = Deaths;
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

        public static bool operator ==(Metrics left, Metrics right) => left.Equals(right);

        public static bool operator !=(Metrics left, Metrics right) => !left.Equals(right);

        public long Confirmed { get; }
        public long Deaths { get; }
        public long Active { get; }
        public long Recovered { get; }

        public static Metrics Empty { get; }
    }
}
