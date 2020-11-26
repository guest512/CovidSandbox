using System.Collections.Generic;
using System.IO;

namespace ReportsGenerator.Data.Providers
{
    public class ModelCacheDataProvider : MultiVersionDataProvider
    {
        public ModelCacheDataProvider()
        {
            VersionFieldsDictionary.Add(RowVersion.ModelCacheData, new List<Field>
            {
                Field.LastUpdate,
                Field.CountryRegion,
                Field.ProvinceState,
                Field.Admin2,
                Field.Confirmed,
                Field.Active,
                Field.Recovered,
                Field.Deaths
            });

            VersionFieldsDictionary.Add(RowVersion.ModelCacheMetaData, new List<Field>
            {
                Field.CountryRegion,
                Field.ProvinceState,
                Field.Admin2,
                Field.ContinentName,
                Field.Population,
                Field.CombinedKey
            });
        }

        protected override string FieldToString(Field field, RowVersion version)
        {
            return field switch
            {
                Field.LastUpdate => "Day",
                Field.CountryRegion => "Country",
                Field.ProvinceState => "Province",
                Field.Admin2 => "County",
                Field.ContinentName => "Continent",
                Field.Population => "Population",
                Field.CombinedKey => "StatsName",
                _ => field.ToString()
            };
        }
    }
}