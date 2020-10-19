namespace ReportsGenerator.Data.Providers
{
    public class MiscDataProvider : MultiVersionDataProvider
    {
        public MiscDataProvider()
        {
            VersionFieldsDictionary.Add(RowVersion.StatsBase, new[]
            {
                Field.UID,
                Field.Iso2,
                Field.Iso3,
                Field.Code3,
                Field.FIPS,
                Field.Admin2,
                Field.ProvinceState,
                Field.CountryRegion,
                Field.Latitude,
                Field.Longitude,
                Field.CombinedKey,
                Field.Population
            });

            VersionFieldsDictionary.Add(RowVersion.StatsEx, new[]
            {
                Field.ContinentName,
                Field.ContinentCode,
                Field.CountryRegion,
                Field.Iso2,
                Field.Iso3,
                Field.Code3
            });

            VersionFieldsDictionary.Add(RowVersion.State, new[]
            {
                Field.Abbreviation,
                Field.Name
            });

            VersionFieldsDictionary.Add(RowVersion.Translation, new[]
            {
                Field.English,
                Field.Russian
            });
        }

        protected override string FieldToString(Field field, RowVersion version)
        {
            return field switch
            {
                Field.Iso2 when version == RowVersion.StatsBase => "iso2",
                Field.Iso3 when version == RowVersion.StatsBase => "iso3",
                Field.Code3 when version == RowVersion.StatsBase => "code3",
                Field.CountryRegion when version == RowVersion.StatsBase => "Country_Region",

                Field.Iso2 when version == RowVersion.StatsEx => "Two_Letter_Country_Code",
                Field.Iso3 when version == RowVersion.StatsEx => "Three_Letter_Country_Code",
                Field.Code3 when version == RowVersion.StatsEx => "Country_Number",
                Field.CountryRegion when version == RowVersion.StatsEx => "Country_Name",

                Field.ProvinceState => "Province_State",
                Field.Latitude => "Lat",
                Field.Longitude => "Long_",
                Field.CombinedKey => "Combined_Key",

                Field.ContinentName => "Continent_Name",
                Field.ContinentCode => "Continent_Code",

                Field.English => "English_JHopkins",
                Field.Russian => "Russian_Yandex",

                _ => field.ToString()
            };
        }
    }
}