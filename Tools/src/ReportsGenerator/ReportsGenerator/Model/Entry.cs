using System;
using ReportsGenerator.Data;

namespace ReportsGenerator.Model
{
    /// <summary>
    /// Represents an object model entity.
    /// Parsed and strongly-typed <see cref="Row"/> representation with comparison capabilities.
    /// </summary>
    public record Entry
    {
        /// <summary>
        /// Gets a key to lookup for additional statistical information.
        /// </summary>
        public string StatsName { get; init; } = string.Empty;

        /// <summary>
        /// Gets an empty (with default values) instance of <see cref="Entry"/>.
        /// </summary>
        public static Entry Empty { get; } = new Entry();

        /// <summary>
        /// Gets a number of active cases.
        /// </summary>
        public long Active { get; init; }

        /// <summary>
        /// Gets a number of confirmed cases.
        /// </summary>
        public long Confirmed { get; init; }

        /// <summary>
        /// Gets a country name.
        /// </summary>
        public string CountryRegion { get; init; } = string.Empty;

        /// <summary>
        /// Gets a county name.
        /// </summary>
        public string County { get; init; } = string.Empty;

        /// <summary>
        /// Gets a number of death cases.
        /// </summary>
        public long Deaths { get; init; }

        /// <summary>
        /// Gets an entry <see cref="IsoLevel"/> value.
        /// </summary>
        public IsoLevel IsoLevel { get; init; }

        /// <summary>
        /// Gets a day for which data is represented.
        /// </summary>
        public DateTime LastUpdate { get; init; }

        /// <summary>
        /// Gets an <see cref="Entry"/> origin.
        /// </summary>
        public Origin Origin { get; init; }

        /// <summary>
        /// Gets a province name.
        /// </summary>
        public string ProvinceState { get; init; } = string.Empty;

        /// <summary>
        /// Gets a number of recovered cases.
        /// </summary>
        public long Recovered { get; init; }

        /// <summary>
        /// Returns the string representation of the <see cref="Entry"/> instance.
        /// </summary>
        /// <returns>The string representation of the instance.</returns>
        public override string ToString() => string.IsNullOrEmpty(ProvinceState)
            ? $"{Origin}-{CountryRegion}, {LastUpdate.ToShortDateString()}: {Confirmed}-{Active}-{Recovered}-{Deaths}"
            : $"{Origin}-{CountryRegion}({ProvinceState}), {LastUpdate.ToShortDateString()}: {Confirmed}-{Active}-{Recovered}-{Deaths}";
    }
}