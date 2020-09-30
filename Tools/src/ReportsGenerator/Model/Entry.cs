﻿using System;
using ReportsGenerator.Data;
using ReportsGenerator.Model.Processors;

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
            FIPS = rowProcessor.GetFips(rowData);
            County = rowProcessor.GetCountyName(rowData);
            Origin = rowProcessor.GetOrigin(rowData);
            IsoLevel = rowProcessor.GetIsoLevel(rowData);
        }

        private Entry(Entry original, long confirmed, long active, long recovered, long deaths)
        {
            ProvinceState = original.ProvinceState;
            CountryRegion = original.CountryRegion;
            LastUpdate = original.LastUpdate;
            FIPS = original.FIPS;
            County = original.County;
            Origin = original.Origin;
            IsoLevel = original.IsoLevel;

            Confirmed = confirmed;
            Active = active;
            Recovered = recovered;
            Deaths = deaths;
        }

        public static Entry Empty { get; }

        public long Active { get; }

        public long Confirmed { get; }

        public string CountryRegion { get; }

        public string County { get; }

        public long Deaths { get; }

        public uint FIPS { get; }

        public IsoLevel IsoLevel { get; }
        public DateTime LastUpdate { get; }

        public Origin Origin { get; }
        public string ProvinceState { get; }

        public long Recovered { get; }

        public static bool operator !=(Entry left, Entry right) => !(left == right);

        public static Entry operator +(Entry left, Entry right)
        {
            if (left == Empty)
                return right;

            if (right == Empty)
                return left;

            if (left.Origin != right.Origin || left.CountryRegion != right.CountryRegion ||
               left.County != right.County || left.LastUpdate != right.LastUpdate ||
               left.FIPS != right.FIPS || left.ProvinceState != right.ProvinceState ||
               left.IsoLevel != right.IsoLevel)
                throw new Exception("Can sum only similar entries");

            return new Entry(left, left.Confirmed + right.Confirmed, left.Active + right.Active,
                left.Recovered + right.Recovered, left.Deaths + right.Deaths);
        }

        public static bool operator ==(Entry left, Entry right) => left.Equals(right);

        public bool Equals(Entry other)
        {
            return IsoLevel == other.IsoLevel && County == other.County && FIPS == other.FIPS && Active == other.Active &&
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
            hashCode.Add(FIPS);
            hashCode.Add(Active);
            hashCode.Add(Confirmed);
            hashCode.Add(CountryRegion);
            hashCode.Add(Deaths);
            hashCode.Add(LastUpdate);
            hashCode.Add(ProvinceState);
            hashCode.Add(Recovered);
            hashCode.Add(Origin);
            hashCode.Add(IsoLevel);
            return hashCode.ToHashCode();
        }

        public override string ToString() => string.IsNullOrEmpty(ProvinceState)
            ? $"{Origin}-{CountryRegion}, {LastUpdate.ToShortDateString()}: {Confirmed}-{Active}-{Recovered}-{Deaths}"
            : $"{Origin}-{CountryRegion}({ProvinceState}), {LastUpdate.ToShortDateString()}: {Confirmed}-{Active}-{Recovered}-{Deaths}";
    }
}