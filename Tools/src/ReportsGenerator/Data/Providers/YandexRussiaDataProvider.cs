namespace ReportsGenerator.Data.Providers
{
    public class YandexRussiaDataProvider : MultiVersionDataProvider
    {
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