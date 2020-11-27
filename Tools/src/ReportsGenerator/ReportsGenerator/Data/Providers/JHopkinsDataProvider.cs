namespace ReportsGenerator.Data.Providers
{
    /// <summary>
    /// Represents an implementation of <see cref="IDataProvider"/> for data from the John Hopkins University.
    /// </summary>
    public class JHopkinsDataProvider : MultiVersionDataProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JHopkinsDataProvider"/> class.
        /// </summary>
        public JHopkinsDataProvider()
        {
            VersionFieldsDictionary.Add(RowVersion.JHopkinsV1, new[]
            {
                FieldId.ProvinceState,
                FieldId.CountryRegion,
                FieldId.LastUpdate,
                FieldId.Confirmed,
                FieldId.Deaths,
                FieldId.Recovered
            });

            VersionFieldsDictionary.Add(RowVersion.JHopkinsV2, new[]
            {
                FieldId.ProvinceState,
                FieldId.CountryRegion,
                FieldId.LastUpdate,
                FieldId.Confirmed,
                FieldId.Deaths,
                FieldId.Recovered,
                FieldId.Latitude,
                FieldId.Longitude
            });

            VersionFieldsDictionary.Add(RowVersion.JHopkinsV3, new[]
            {
                FieldId.FIPS,
                FieldId.Admin2,
                FieldId.ProvinceState,
                FieldId.CountryRegion,
                FieldId.LastUpdate,
                FieldId.Latitude,
                FieldId.Longitude,
                FieldId.Confirmed,
                FieldId.Deaths,
                FieldId.Recovered,
                FieldId.Active,
                FieldId.CombinedKey
            });

            VersionFieldsDictionary.Add(RowVersion.JHopkinsV4, new[]
            {
                FieldId.FIPS,
                FieldId.Admin2,
                FieldId.ProvinceState,
                FieldId.CountryRegion,
                FieldId.LastUpdate,
                FieldId.Latitude,
                FieldId.Longitude,
                FieldId.Confirmed,
                FieldId.Deaths,
                FieldId.Recovered,
                FieldId.Active,
                FieldId.CombinedKey,
                FieldId.IncidenceRate,
                FieldId.CaseFatalityRatio
            });

            VersionFieldsDictionary.Add(RowVersion.JHopkinsV5, new[]
            {
                FieldId.FIPS,
                FieldId.Admin2,
                FieldId.ProvinceState,
                FieldId.CountryRegion,
                FieldId.LastUpdate,
                FieldId.Latitude,
                FieldId.Longitude,
                FieldId.Confirmed,
                FieldId.Deaths,
                FieldId.Recovered,
                FieldId.Active,
                FieldId.CombinedKey,
                FieldId.IncidenceRate,
                FieldId.CaseFatalityRatio
            });
        }

        /// <inheritdoc />
        protected override string FieldToString(FieldId field, RowVersion version)
        {
            return field switch
            {
                FieldId.ProvinceState when version == RowVersion.JHopkinsV1 || version == RowVersion.JHopkinsV2 => "Province/State",
                FieldId.ProvinceState => "Province_State",

                FieldId.CountryRegion when version == RowVersion.JHopkinsV1 || version == RowVersion.JHopkinsV2 => "Country/Region",
                FieldId.CountryRegion => "Country_Region",

                FieldId.LastUpdate when version == RowVersion.JHopkinsV1 || version == RowVersion.JHopkinsV2 => "Last Update",
                FieldId.LastUpdate => "Last_Update",

                FieldId.Latitude when version == RowVersion.JHopkinsV1 || version == RowVersion.JHopkinsV2 => field.ToString(),
                FieldId.Latitude => "Lat",

                FieldId.Longitude when version == RowVersion.JHopkinsV1 || version == RowVersion.JHopkinsV2 => field.ToString(),
                FieldId.Longitude => "Long_",

                FieldId.CombinedKey => "Combined_Key",

                FieldId.IncidenceRate when version == RowVersion.JHopkinsV4 => "Incidence_Rate",
                FieldId.IncidenceRate when version == RowVersion.JHopkinsV5 => "Incident_Rate",

                FieldId.CaseFatalityRatio when version == RowVersion.JHopkinsV4 => "Case-Fatality_Ratio",
                FieldId.CaseFatalityRatio when version == RowVersion.JHopkinsV5 => "Case_Fatality_Ratio",


                _ => field.ToString()
            };
        }
    }
}