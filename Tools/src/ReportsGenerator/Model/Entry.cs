using System;
using ReportsGenerator.Data;
using ReportsGenerator.Model.Processors;

namespace ReportsGenerator.Model
{
    /// <summary>
    /// Represents an object model entity.
    /// Parsed and strongly-typed <see cref="Row"/> representation with comparison capabilities.
    /// </summary>
    public readonly struct Entry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Entry"/> structure.
        /// </summary>
        /// <param name="rowData">Data source for the instance.</param>
        /// <param name="rowProcessor">Data accessor.</param>
        public Entry(Row rowData, IRowProcessor rowProcessor)
        {
            ProvinceState = rowProcessor.GetProvinceName(rowData);
            CountryRegion = rowProcessor.GetCountryName(rowData);
            LastUpdate = rowProcessor.GetLastUpdate(rowData);
            Confirmed = rowProcessor.GetConfirmed(rowData);
            Deaths = rowProcessor.GetDeaths(rowData);
            Recovered = rowProcessor.GetRecovered(rowData);
            Active = rowProcessor.GetActive(rowData);
            County = rowProcessor.GetCountyName(rowData);
            Origin = rowProcessor.GetOrigin(rowData);
            IsoLevel = rowProcessor.GetIsoLevel(rowData);
            StatsName = rowProcessor.GetStatsName(rowData);
        }

        /// <summary>
        /// Gets an empty (with default values) instance of <see cref="Entry"/>.
        /// </summary>
        public static Entry Empty { get; }

        /// <summary>
        /// Gets a number of active cases.
        /// </summary>
        public long Active { get; }

        /// <summary>
        /// Gets a number of confirmed cases.
        /// </summary>
        public long Confirmed { get; }

        /// <summary>
        /// Gets a country name.
        /// </summary>
        public string CountryRegion { get; }

        /// <summary>
        /// Gets a county name.
        /// </summary>
        public string County { get; }

        /// <summary>
        /// Gets a number of death cases.
        /// </summary>
        public long Deaths { get; }

        /// <summary>
        /// Gets an entry <see cref="IsoLevel"/> value.
        /// </summary>
        public IsoLevel IsoLevel { get; }

        /// <summary>
        /// Gets a day for which data is represented.
        /// </summary>
        public DateTime LastUpdate { get; }

        /// <summary>
        /// Gets an <see cref="Entry"/> origin.
        /// </summary>
        public Origin Origin { get; }

        /// <summary>
        /// Gets a province name.
        /// </summary>
        public string ProvinceState { get; }

        /// <summary>
        /// Gets a number of recovered cases.
        /// </summary>
        public long Recovered { get; }

        /// <summary>
        /// Gets a key to lookup for additional statistical information.
        /// </summary>
        public string StatsName { get; }

        /// <summary>
        /// Compares two <see cref="Entry"/> instances for inequality.
        /// </summary>
        /// <param name="left">First <see cref="Entry"/> instance to compare.</param>
        /// <param name="right">Second <see cref="Entry"/> instance to compare.</param>
        /// <returns><see langword="true" /> if instances has the different property values, otherwise returns <see langword="false" />.</returns>
        public static bool operator !=(Entry left, Entry right) => !(left == right);

        /// <summary>
        /// Compares two <see cref="Metrics"/> instances for equality.
        /// </summary>
        /// <param name="left">First <see cref="Metrics"/> instance to compare.</param>
        /// <param name="right">Second <see cref="Metrics"/> instance to compare.</param>
        /// <returns><see langword="true" /> if instances has the same property values, otherwise returns <see langword="false" />.</returns>
        public static bool operator ==(Entry left, Entry right) => left.Equals(right);

        /// <inheritdoc cref="Entry.Equals(object?)"/>
        public bool Equals(Entry other)
        {
            return IsoLevel == other.IsoLevel && County == other.County && StatsName == other.StatsName && Active == other.Active &&
                   Confirmed == other.Confirmed && CountryRegion == other.CountryRegion && Deaths == other.Deaths &&
                   LastUpdate.Equals(other.LastUpdate) && ProvinceState == other.ProvinceState &&
                   Recovered == other.Recovered && Origin == other.Origin;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is Entry other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(County);
            hashCode.Add(Active);
            hashCode.Add(Confirmed);
            hashCode.Add(CountryRegion);
            hashCode.Add(Deaths);
            hashCode.Add(LastUpdate);
            hashCode.Add(ProvinceState);
            hashCode.Add(Recovered);
            hashCode.Add(Origin);
            hashCode.Add(IsoLevel);
            hashCode.Add(StatsName);
            return hashCode.ToHashCode();
        }


        /// <summary>
        /// Returns the string representation of the <see cref="Entry"/> instance.
        /// </summary>
        /// <returns>The string representation of the instance.</returns>
        public override string ToString() => string.IsNullOrEmpty(ProvinceState)
            ? $"{Origin}-{CountryRegion}, {LastUpdate.ToShortDateString()}: {Confirmed}-{Active}-{Recovered}-{Deaths}"
            : $"{Origin}-{CountryRegion}({ProvinceState}), {LastUpdate.ToShortDateString()}: {Confirmed}-{Active}-{Recovered}-{Deaths}";
    }
}