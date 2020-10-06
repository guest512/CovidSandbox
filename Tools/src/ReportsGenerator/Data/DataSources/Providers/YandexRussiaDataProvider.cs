using System.Collections.Generic;
using System.Linq;

namespace ReportsGenerator.Data.DataSources.Providers
{
    public class YandexRussiaDataProvider : IDataProvider
    {
        private readonly IEnumerable<Field> _headerFields = new[]
        {
            Field.LastUpdate,
            Field.ProvinceState,
            Field.Confirmed,
            Field.Recovered,
            Field.Deaths,
            Field.DeathsByDay,
            Field.ConfirmedByDay,
            Field.RecoveredByDay
        };

        public IEnumerable<Field> GetFields(RowVersion version) =>
            version == RowVersion.YandexRussia
                ? _headerFields
                : Enumerable.Empty<Field>();

        public RowVersion GetVersion(IEnumerable<string> header)
        {
            var headerEnumerator = header.GetEnumerator();
            using var fieldsEnumerator = _headerFields.GetEnumerator();

            while (headerEnumerator.MoveNext() && fieldsEnumerator.MoveNext())
            {
                if (headerEnumerator.Current! != FieldToString(fieldsEnumerator.Current))
                    return RowVersion.Unknown;
            }

            return RowVersion.YandexRussia;
        }

        private static string FieldToString(Field field)
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