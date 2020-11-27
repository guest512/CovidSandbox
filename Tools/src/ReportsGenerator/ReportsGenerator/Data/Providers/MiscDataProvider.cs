namespace ReportsGenerator.Data.Providers
{
    /// <summary>
    /// Represents an implementation of <see cref="IDataProvider"/> for helper and statistical data.
    /// </summary>
    public class MiscDataProvider : MultiVersionDataProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MiscDataProvider"/> class.
        /// </summary>
        public MiscDataProvider()
        {
            VersionFieldsDictionary.Add(RowVersion.StatsBase, new[]
            {
                FieldId.UID,
                FieldId.Iso2,
                FieldId.Iso3,
                FieldId.Code3,
                FieldId.FIPS,
                FieldId.Admin2,
                FieldId.ProvinceState,
                FieldId.CountryRegion,
                FieldId.Latitude,
                FieldId.Longitude,
                FieldId.CombinedKey,
                FieldId.Population
            });

            VersionFieldsDictionary.Add(RowVersion.StatsEx, new[]
            {
                FieldId.ContinentName,
                FieldId.ContinentCode,
                FieldId.CountryRegion,
                FieldId.Iso2,
                FieldId.Iso3,
                FieldId.Code3
            });

            VersionFieldsDictionary.Add(RowVersion.State, new[]
            {
                FieldId.Abbreviation,
                FieldId.Name
            });

            VersionFieldsDictionary.Add(RowVersion.Translation, new[]
            {
                FieldId.English,
                FieldId.Russian
            });
        }

        /// <inheritdoc />
        protected override string FieldToString(FieldId field, RowVersion version)
        {
            return field switch
            {
                FieldId.Iso2 when version == RowVersion.StatsBase => "iso2",
                FieldId.Iso3 when version == RowVersion.StatsBase => "iso3",
                FieldId.Code3 when version == RowVersion.StatsBase => "code3",
                FieldId.CountryRegion when version == RowVersion.StatsBase => "Country_Region",

                FieldId.Iso2 when version == RowVersion.StatsEx => "Two_Letter_Country_Code",
                FieldId.Iso3 when version == RowVersion.StatsEx => "Three_Letter_Country_Code",
                FieldId.Code3 when version == RowVersion.StatsEx => "Country_Number",
                FieldId.CountryRegion when version == RowVersion.StatsEx => "Country_Name",

                FieldId.ProvinceState => "Province_State",
                FieldId.Latitude => "Lat",
                FieldId.Longitude => "Long_",
                FieldId.CombinedKey => "Combined_Key",

                FieldId.ContinentName => "Continent_Name",
                FieldId.ContinentCode => "Continent_Code",

                FieldId.English => "English_JHopkins",
                FieldId.Russian => "Russian_Yandex",

                _ => field.ToString()
            };
        }
    }
}