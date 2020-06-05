using System;
using System.Collections.Generic;
using System.Text;

namespace CovidSandbox.Model
{
    public readonly struct Metrics
    {
        public Metrics(in uint confirmed, in uint active, in uint recovered, in uint deaths)
        {
            Confirmed = confirmed;
            Deaths = deaths;
            Active = active;
            Recovered = recovered;
        }

        public void Deconstruct(out uint confirmed, out uint active, out uint recovered, out uint deaths)
        {
            confirmed = Confirmed;
            active = Active;
            recovered = Recovered;
            deaths = Deaths;
        }

        public uint Confirmed { get; }
        public uint Deaths { get; }
        public uint Active { get; }
        public uint Recovered { get; }

        public static Metrics Empty { get; }
    }
}
