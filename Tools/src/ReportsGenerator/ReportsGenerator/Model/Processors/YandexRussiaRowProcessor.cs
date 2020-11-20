using ReportsGenerator.Data;
using ReportsGenerator.Utils;

namespace ReportsGenerator.Model.Processors
{
    /// <summary>
    /// Represents a <see cref="IRowProcessor"/> implementation for Yandex data for Russia.
    /// Based on <see cref="BaseRowProcessor"/> implementation.
    /// </summary>
    public sealed class YandexRussiaRowProcessor : BaseRowProcessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="YandexRussiaRowProcessor"/> class.
        /// </summary>
        /// <param name="statsProvider">Statistical provider to generate key.</param>
        /// <param name="logger">A <see cref="ILogger"/> instance.</param>
        public YandexRussiaRowProcessor(IStatsProvider statsProvider, ILogger logger) : base(statsProvider, logger)
        {
        }

        /// <inheritdoc />
        public override string GetCountryName(Row row) => "Russia";

        /// <inheritdoc />
        public override string GetCountyName(Row row) => string.Empty;

        /// <inheritdoc />
        public override Origin GetOrigin(Row row) => Origin.Yandex;

        /// <inheritdoc />
        public override string GetProvinceName(Row row) => row[Field.ProvinceState];
    }
}