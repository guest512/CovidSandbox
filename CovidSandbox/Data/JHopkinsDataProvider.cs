using System;
using System.Collections.Generic;
using System.Linq;

namespace CovidSandbox.Data
{
    public interface IDataProvider
    {
        IEnumerable<Field> GetFields(RowVersion version);

        RowVersion GetVersion(string[] header);
    }

    internal class JHopkinsDataProvider : IDataProvider
    {
        private readonly Dictionary<RowVersion, IEnumerable<Field>> _versionFieldsDictionary =
            new Dictionary<RowVersion, IEnumerable<Field>>()
            {
                {
                    RowVersion.JHopkinsV1, new[]
                    {
                        Field.ProvinceState,
                        Field.CountryRegion,
                        Field.LastUpdate,
                        Field.Confirmed,
                        Field.Deaths,
                        Field.Recovered
                    }
                },
                {
                    RowVersion.JHopkinsV2, new []
                    {
                        Field.ProvinceState,
                        Field.CountryRegion,
                        Field.LastUpdate,
                        Field.Confirmed,
                        Field.Deaths,
                        Field.Recovered,
                        Field.Latitude,
                        Field.Longitude
                    }
                },
                {
                    RowVersion.JHopkinsV3, new []
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
                    }
                },
                {
                    RowVersion.JHopkinsV4, new []
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
                        Field.CombinedKey,
                        Field.IncidenceRate,
                        Field.CaseFatalityRatio
                    }
                }
            };

        public IEnumerable<Field> GetFields(RowVersion version)
        {
            if (_versionFieldsDictionary.ContainsKey(version))
                return _versionFieldsDictionary[version];
            throw new ArgumentOutOfRangeException(nameof(version), "Unsupported version by this data provider");
        }

        public RowVersion GetVersion(string[] header)
        {
            var headersCount = header.Length;

            var (rowVersion, fields) = _versionFieldsDictionary
                .FirstOrDefault(_ => headersCount == _.Value.Count() && ValidateColumnsOrder(_.Key, header));

            return fields == null ? RowVersion.Unknown : rowVersion;
        }

        private bool ValidateColumnsOrder(RowVersion version, IEnumerable<string> headerColumns)
        {
            using var headerEnumerator = headerColumns.GetEnumerator();
            using var fieldsEnumerator = _versionFieldsDictionary[version].GetEnumerator();

            while (headerEnumerator.MoveNext() && fieldsEnumerator.MoveNext())
            {
                if (headerEnumerator.Current != FieldToString(fieldsEnumerator.Current, version))
                    return false;
            }

            return true;
        }

        private string FieldToString(Field field, RowVersion version)
        {
            return field switch
            {
                Field.ProvinceState when version == RowVersion.JHopkinsV1 || version == RowVersion.JHopkinsV2 => "Province/State",
                Field.ProvinceState => "Province_State",

                Field.CountryRegion when version == RowVersion.JHopkinsV1 || version == RowVersion.JHopkinsV2 => "Country/Region",
                Field.CountryRegion => "Country_Region",

                Field.LastUpdate when version == RowVersion.JHopkinsV1 || version == RowVersion.JHopkinsV2 => "Last Update",
                Field.LastUpdate => "Last_Update",

                Field.Latitude when version == RowVersion.JHopkinsV1 || version == RowVersion.JHopkinsV2 => field.ToString(),
                Field.Latitude => "Lat",

                Field.Longitude when version == RowVersion.JHopkinsV1 || version == RowVersion.JHopkinsV2 => field.ToString(),
                Field.Longitude => "Long_",

                Field.CombinedKey => "Combined_Key",

                Field.IncidenceRate => "Incidence_Rate",

                Field.CaseFatalityRatio => "Case-Fatality_Ratio",

                _ => field.ToString()
            };
        }
    }
}