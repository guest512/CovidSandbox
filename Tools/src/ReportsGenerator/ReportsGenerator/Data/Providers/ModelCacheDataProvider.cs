using System.Collections.Generic;

namespace ReportsGenerator.Data.Providers
{
    /// <summary>
    /// Represents an implementation of <see cref="IDataProvider"/> for data model cache.
    /// </summary>
    public class ModelCacheDataProvider : MultiVersionDataProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModelCacheDataProvider"/> class.
        /// </summary>
        public ModelCacheDataProvider()
        {
            VersionFieldsDictionary.Add(RowVersion.ModelCacheData, new List<FieldId>
            {
                FieldId.LastUpdate,
                FieldId.CountryRegion,
                FieldId.ProvinceState,
                FieldId.Admin2,
                FieldId.Confirmed,
                FieldId.Active,
                FieldId.Recovered,
                FieldId.Deaths
            });

            VersionFieldsDictionary.Add(RowVersion.ModelCacheMetaData, new List<FieldId>
            {
                FieldId.CountryRegion,
                FieldId.ProvinceState,
                FieldId.Admin2,
                FieldId.ContinentName,
                FieldId.Population,
                FieldId.CombinedKey
            });
        }

        /// <inheritdoc />
        protected override string FieldToString(FieldId field, RowVersion version)
        {
            return field switch
            {
                FieldId.LastUpdate => "Day",
                FieldId.CountryRegion => "Country",
                FieldId.ProvinceState => "Province",
                FieldId.Admin2 => "County",
                FieldId.ContinentName => "Continent",
                FieldId.Population => "Population",
                FieldId.CombinedKey => "StatsName",
                _ => field.ToString()
            };
        }
    }
}