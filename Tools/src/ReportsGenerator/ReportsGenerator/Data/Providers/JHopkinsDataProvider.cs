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
                Field.ProvinceState,
                Field.CountryRegion,
                Field.LastUpdate,
                Field.Confirmed,
                Field.Deaths,
                Field.Recovered
            });

            VersionFieldsDictionary.Add(RowVersion.JHopkinsV2, new[]
            {
                Field.ProvinceState,
                Field.CountryRegion,
                Field.LastUpdate,
                Field.Confirmed,
                Field.Deaths,
                Field.Recovered,
                Field.Latitude,
                Field.Longitude
            });

            VersionFieldsDictionary.Add(RowVersion.JHopkinsV3, new[]
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
            });

            VersionFieldsDictionary.Add(RowVersion.JHopkinsV4, new[]
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
            });

            VersionFieldsDictionary.Add(RowVersion.JHopkinsV5, new[]
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
            });
        }

        /// <inheritdoc />
        protected override string FieldToString(Field field, RowVersion version)
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

                Field.IncidenceRate when version == RowVersion.JHopkinsV4 => "Incidence_Rate",
                Field.IncidenceRate when version == RowVersion.JHopkinsV5 => "Incident_Rate",

                Field.CaseFatalityRatio when version == RowVersion.JHopkinsV4 => "Case-Fatality_Ratio",
                Field.CaseFatalityRatio when version == RowVersion.JHopkinsV5 => "Case_Fatality_Ratio",


                _ => field.ToString()
            };
        }
    }
}