using ReportsGenerator.Data;
using ReportsGenerator.Utils;

namespace ReportsGenerator.Model.Processors
{
    public sealed class YandexRussiaRowProcessor : BaseRowProcessor
    {
        public YandexRussiaRowProcessor(ILogger logger) : base(logger)
        {
        }

        public override string GetCountryName(Row row) => "Russia";

        public override string GetCountyName(Row row) => string.Empty;

        public override uint GetFips(Row row) => 0;

        public override Origin GetOrigin(Row row) => Origin.Yandex;

        public override string GetProvinceName(Row row) => row[Field.ProvinceState];
    }
}