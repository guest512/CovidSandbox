﻿using ReportsGenerator.Data;
using ReportsGenerator.Model.Processors;
using System;

namespace ReportsGenerator.Model
{
    public readonly struct Entry
    {
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

        public string StatsName { get; }

        public static Entry Empty { get; }

        public long Active { get; }

        public long Confirmed { get; }

        public string CountryRegion { get; }

        public string County { get; }

        public long Deaths { get; }

        public IsoLevel IsoLevel { get; }

        public DateTime LastUpdate { get; }

        public Origin Origin { get; }

        public string ProvinceState { get; }

        public long Recovered { get; }

        public static bool operator !=(Entry left, Entry right) => !(left == right);

        public static bool operator ==(Entry left, Entry right) => left.Equals(right);

        public bool Equals(Entry other)
        {
            return IsoLevel == other.IsoLevel && County == other.County && StatsName == other.StatsName && Active == other.Active &&
                   Confirmed == other.Confirmed && CountryRegion == other.CountryRegion && Deaths == other.Deaths &&
                   LastUpdate.Equals(other.LastUpdate) && ProvinceState == other.ProvinceState &&
                   Recovered == other.Recovered && Origin == other.Origin;
        }

        public override bool Equals(object? obj)
        {
            return obj is Entry other && Equals(other);
        }

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

        public override string ToString() => string.IsNullOrEmpty(ProvinceState)
            ? $"{Origin}-{CountryRegion}, {LastUpdate.ToShortDateString()}: {Confirmed}-{Active}-{Recovered}-{Deaths}"
            : $"{Origin}-{CountryRegion}({ProvinceState}), {LastUpdate.ToShortDateString()}: {Confirmed}-{Active}-{Recovered}-{Deaths}";
    }
}