namespace ReportsGenerator.Data.Providers
{
    /// <summary>
    /// Represents an implementation of <see cref="IDataProvider"/> for data from Yandex.
    /// </summary>
    public class YandexRussiaDataProvider : MultiVersionDataProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="YandexRussiaDataProvider"/> class.
        /// </summary>
        public YandexRussiaDataProvider()
        {
            VersionFieldsDictionary.Add(RowVersion.YandexRussia, new[]
            {
                FieldId.LastUpdate,
                FieldId.ProvinceState,
                FieldId.Confirmed,
                FieldId.Recovered,
                FieldId.Deaths,
                FieldId.DeathsByDay,
                FieldId.ConfirmedByDay,
                FieldId.RecoveredByDay
            });
        }

        /// <inheritdoc />
        protected override string FieldToString(FieldId field, RowVersion version)
        {
            return field switch
            {
                FieldId.Confirmed => "Заражений",
                FieldId.LastUpdate => "Дата",
                FieldId.ProvinceState => "Регион",
                FieldId.Recovered => "Выздоровлений",
                FieldId.Deaths => "Смертей",
                FieldId.DeathsByDay => "Смертей за день",
                FieldId.ConfirmedByDay => "Заражений за день",
                FieldId.RecoveredByDay => "Выздоровлений за день",

                _ => field.ToString()
            };
        }
    }
}