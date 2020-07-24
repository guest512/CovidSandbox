using CovidSandbox.Data;

namespace CovidSandbox.Model.Processors
{
    public sealed class YandexRussiaRowProcessor : BaseRowProcessor
    {
        public override string GetCountryName(Row row) => "Russia";

        public override string GetCountyName(Row row) => string.Empty;

        public override uint GetFips(Row row) => 0;

        public override Origin GetOrigin(Row row) => Origin.Yandex;

        public override string GetProvinceName(Row row)
        {
            var province = row[Field.ProvinceState];

            return province switch
            {
                _ => province
            };
        }
    }
}