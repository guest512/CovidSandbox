using System;

namespace ReportsGenerator.Model
{
    public record Entry
    {
        public string StatsName { get; init; } = string.Empty;

        public static Entry Empty { get; } = new Entry();

        public long Active { get; init; }

        public long Confirmed { get; init; }

        public string CountryRegion { get; init; } = string.Empty;

        public string County { get; init; } = string.Empty;

        public long Deaths { get; init; }

        public IsoLevel IsoLevel { get; init; }

        public DateTime LastUpdate { get; init; }

        public Origin Origin { get; init; }

        public string ProvinceState { get; init; } = string.Empty;

        public long Recovered { get; init; }

        public override string ToString() => string.IsNullOrEmpty(ProvinceState)
            ? $"{Origin}-{CountryRegion}, {LastUpdate.ToShortDateString()}: {Confirmed}-{Active}-{Recovered}-{Deaths}"
            : $"{Origin}-{CountryRegion}({ProvinceState}), {LastUpdate.ToShortDateString()}: {Confirmed}-{Active}-{Recovered}-{Deaths}";
    }
}