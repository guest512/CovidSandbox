using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ReportsGenerator;
using ReportsGenerator.Data;
using ReportsGenerator.Data.DataSources;
using ReportsGenerator.Data.IO.Csv;
using ReportsGenerator.Model;
using ReportsGenerator.Model.Processors;
using ReportsGenerator.Model.Reports;
using ReportsGenerator.Utils;

const string dataRoot = "Data";

static string GetDataFolder<T>() where T : IDataSource
{
    if (typeof(T) == typeof(JHopkinsDataSource))
    {
        return Path.Combine(dataRoot, "JHopkins");
    }

    if (typeof(T) == typeof(YandexRussiaDataSource))
    {
        return Path.Combine(dataRoot, "Yandex");
    }

    if (typeof(T) == typeof(MiscDataSource))
    {
        return Path.Combine(dataRoot, "Misc");
    }

    if (typeof(T) == typeof(ModelCacheDataSource))
    {
        return Path.Combine(dataRoot, ".cache");
    }

    throw new ArgumentOutOfRangeException(nameof(T));
}

static Dictionary<RowVersion, IRowProcessor> GetRowProcessors(INames namesService, IStatsProvider statsProvider, ILogger logger)
{
    var jHopkinsProcessor = new JHopkinsRowProcessor(namesService, statsProvider, logger);

    var rowProcessors = new Dictionary<RowVersion, IRowProcessor>
            {
                {RowVersion.JHopkinsV1, jHopkinsProcessor},
                {RowVersion.JHopkinsV2, jHopkinsProcessor},
                {RowVersion.JHopkinsV3, jHopkinsProcessor},
                {RowVersion.JHopkinsV4, jHopkinsProcessor},
                {RowVersion.JHopkinsV5, jHopkinsProcessor},
                {RowVersion.YandexRussia, new YandexRussiaRowProcessor(statsProvider, logger)}
            };

    return rowProcessors;
}

static void SaveReports(string dest, ReportsBuilder reportsBuilder, ILogger logger)
{
    using ReportsSaver<string> reportsSaver = new(new CsvReportFormatter(), new CsvFileReportStorage(dest, true), new NullLogger());

    logger.WriteInfo("Create day by day reports...");
    Parallel.ForEach(reportsBuilder.AvailableDates.Select(reportsBuilder.GetDayReport),
        reportsSaver.WriteReport);

    logger.WriteInfo("Create country reports...");
    var countryReports = reportsBuilder.AvailableCountries.Select(reportsBuilder.GetCountryReport).ToArray();

    Parallel.ForEach(countryReports, reportsSaver.WriteReport);
    Parallel.ForEach(countryReports.SelectMany(cr => cr.RegionReports), reportsSaver.WriteReport);

    logger.WriteInfo("Create country stats...");
    var countryStats = reportsBuilder.AvailableCountries.Select(reportsBuilder.GetCountryStats).ToArray();

    Parallel.ForEach(countryStats.Select(statsWalker => statsWalker.Country), reportsSaver.WriteReport); //Country
    Parallel.ForEach(
        countryStats.SelectMany(statsWalker => statsWalker.Provinces)
            .Where(r => r.Name != Consts.MainCountryRegion && r.Name != Consts.OtherCountryRegion),
        reportsSaver.WriteReport); //Region
    Parallel.ForEach(
        countryStats.SelectMany(statsWalker =>
            statsWalker.Provinces.Where(r => r.Name != Consts.MainCountryRegion && r.Name != Consts.OtherCountryRegion)
                .SelectMany(province => statsWalker.GetCounties(province.Name))), reportsSaver.WriteReport); // County
}

static void UpdateCache(string cacheLocation, ReportsBuilder reportsBuilder, ILogger logger)
{
    using ReportsSaver<string> reportsSaver = new(new CsvReportFormatter(), new CsvFileReportStorage(cacheLocation, false), new NullLogger());
    logger.WriteInfo("Create model cache...");
    Parallel.ForEach(reportsBuilder.DumpModelData(), reportsSaver.WriteReport);
    Parallel.ForEach(reportsBuilder.DumpModelMetadata(), reportsSaver.WriteReport);

}

var logger = new ConsoleLogger();


try
{
    logger.WriteInfo("Logger created");
    Convertors.SetLogger(logger);

    var argsParser = new ArgsParser(args, logger);

    var parsedData = new List<Entry>();

    var dataSources = new IDataSource[]
    {
                    new JHopkinsDataSource(GetDataFolder<JHopkinsDataSource>(), logger),
                    new YandexRussiaDataSource(GetDataFolder<YandexRussiaDataSource>(), logger)
    };

    var miscStorage = new MiscStorage(new MiscDataSource(GetDataFolder<MiscDataSource>(), logger), logger);
    var entryFactory = new EntryFactory(GetRowProcessors(miscStorage, miscStorage, logger), logger);
    
    var reportsBuilder = new ReportsBuilder(miscStorage, logger);

    logger.WriteInfo("Initialize cache...");
    reportsBuilder.InitializeCache(new ModelCacheDataSource(GetDataFolder<ModelCacheDataSource>(), logger),out var lastDay);

    logger.WriteInfo("Reading raw data...");
    logger.IndentIncrease();

    foreach (var ds in dataSources)
    {
        await foreach (var row in ds.GetReader()
            .GetRowsAsync(
                field => field.Id == FieldId.LastUpdate && field.Value.AsDate() > lastDay,
                entryFactory.CreateEntry))
        {
            parsedData.Add(row);
        }
    }

    logger.IndentDecrease();

    logger.WriteInfo("Initialize reports generator...");
    logger.IndentIncrease();

    reportsBuilder.AddEntries(parsedData);
    reportsBuilder.Build();

    logger.IndentDecrease();

    SaveReports(argsParser.ReportsDir, reportsBuilder, logger);
    UpdateCache(dataRoot, reportsBuilder, logger);
}
catch (Exception ex)
{
    while (logger.Indentation > 0)
    {
        logger.IndentDecrease();
    }

    logger.WriteError(ex.ToString());
    return 1;
}
finally
{
    Convertors.SetLogger(new NullLogger());
    logger.Dispose();
}

return 0;