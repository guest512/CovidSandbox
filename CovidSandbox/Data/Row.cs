using System;
using System.Collections.Generic;

namespace CovidSandbox.Data
{
    internal class Row
    {
        private readonly Dictionary<Field, string> _data = new Dictionary<Field, string>();

        public Row(string rawData, RowVersion version)
        {
            var dataFields = Utils.SplitCsvRowString(rawData);
            InitializeData(dataFields, version);
        }

        public string this[Field key] => _data.ContainsKey(key) ? _data[key] : string.Empty;

        private void InitializeData(string[] fields, RowVersion version)
        {
            switch (version)
            {
                case RowVersion.V1:
                    InitializeDataV1(fields);
                    break;

                case RowVersion.V2:
                    InitializeDataV2(fields);
                    break;

                case RowVersion.V3:
                    InitializeDataV3(fields);
                    break;

                default:
                    throw new ArgumentException("Unsupported fields version", nameof(version));
            }
        }

        private void InitializeDataV1(IEnumerable<string> fields)
        {
            ReadData(new[]
            {
                Field.ProvinceState,
                Field.CountryRegion,
                Field.LastUpdate,
                Field.Confirmed,
                Field.Deaths,
                Field.Recovered
            }, fields);
        }

        private void InitializeDataV2(IEnumerable<string> fields)
        {
            ReadData(new[]
            {
                Field.ProvinceState,
                Field.CountryRegion,
                Field.LastUpdate,
                Field.Confirmed,
                Field.Deaths,
                Field.Recovered,
                Field.Latitude,
                Field.Longitude
            }, fields);
        }

        private void InitializeDataV3(IEnumerable<string> fields)
        {
            ReadData(new[]
            {
                Field.FIPS,
                Field.Admin2,
                Field.ProvinceState,
                Field.CountryRegion,
                Field.LastUpdate,
                Field.Latitude,
                Field.Longitude,
                Field.Confirmed,
                Field.Deaths,
                Field.Recovered,
                Field.Active,
                Field.CombinedKey
            }, fields);
        }

        private void ReadData(IEnumerable<Field> keys, IEnumerable<string> fields)
        {
            using var keyEnumerator = keys.GetEnumerator();
            using var fieldsEnumerator = fields.GetEnumerator();

            while (keyEnumerator.MoveNext() && fieldsEnumerator.MoveNext())
            {
                _data[keyEnumerator.Current] = fieldsEnumerator.Current;
            }
        }
    }
}