namespace ReportsGenerator.Data.Providers
{
    public class JHopkinsDataProvider : MultiVersionDataProvider
    {
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
        }

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

                Field.IncidenceRate => "Incidence_Rate",

                Field.CaseFatalityRatio => "Case-Fatality_Ratio",

                _ => field.ToString()
            };
        }
    }
}