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
                Field.LastUpdate,
                Field.ProvinceState,
                Field.Confirmed,
                Field.Recovered,
                Field.Deaths,
                Field.DeathsByDay,
                Field.ConfirmedByDay,
                Field.RecoveredByDay
            });
        }

        /// <inheritdoc />
        protected override string FieldToString(Field field, RowVersion version)
        {
            return field switch
            {
                Field.Confirmed => "Заражений",
                Field.LastUpdate => "Дата",
                Field.ProvinceState => "Регион",
                Field.Recovered => "Выздоровлений",
                Field.Deaths => "Смертей",
                Field.DeathsByDay => "Смертей за день",
                Field.ConfirmedByDay => "Заражений за день",
                Field.RecoveredByDay => "Выздоровлений за день",

                _ => field.ToString()
            };
        }
    }
}