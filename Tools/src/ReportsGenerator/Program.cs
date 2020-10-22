using ReportsGenerator.Data;
using ReportsGenerator.Data.DataSources;
using ReportsGenerator.Data.IO;
using ReportsGenerator.Model;
using ReportsGenerator.Model.Processors;
using ReportsGenerator.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ReportsGenerator.Model.Reports;

namespace ReportsGenerator
{
    internal static class Program
    {
        private static string GetDataFolder<T>() where T : IDataSource
        {
            const string dataRoot = "Data";

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

            throw new ArgumentOutOfRangeException(nameof(T));
        }

        private static Dictionary<RowVersion, IRowProcessor> GetRowProcessors(INames namesService, IStatsProvider statsProvider, ILogger logger)
        {
            var jHopkinsProcessor = new JHopkinsRowProcessor(namesService, statsProvider, logger);

            var rowProcessors = new Dictionary<RowVersion, IRowProcessor>
            {
                {RowVersion.JHopkinsV1, jHopkinsProcessor},
                {RowVersion.JHopkinsV2, jHopkinsProcessor},
                {RowVersion.JHopkinsV3, jHopkinsProcessor},
                {RowVersion.JHopkinsV4, jHopkinsProcessor},
                {RowVersion.YandexRussia, new YandexRussiaRowProcessor(statsProvider, logger)}
            };

            return rowProcessors;
        }

        private static async Task<int> Main(string[] args)
        {
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
                var reportsSaver = new ReportsSaver(new StringReportFormatter(), new CsvFileReportStorage(argsParser.ReportsDir, true), new NullLogger());
                var reportsBuilder = new ReportsBuilder(logger);

                logger.WriteInfo("Reading raw data...");
                logger.IndentIncrease();

                miscStorage.Init();
                foreach (var ds in dataSources)
                {
                    await foreach (var row in ds.GetReader().GetRowsAsync(entryFactory.CreateEntry))
                    {
                        parsedData.Add(row);
                    }
                }

                logger.IndentDecrease();

                logger.WriteInfo("Initialize reports generator...");
                logger.IndentIncrease();

                reportsBuilder.AddEntries(parsedData);
                reportsBuilder.Build(miscStorage);

                logger.IndentDecrease();

                logger.WriteInfo("Create day by day reports...");
                Parallel.ForEach(reportsBuilder.AvailableDates.Select(reportsBuilder.GetDayReport),
                    reportsSaver.WriteReport);

                logger.WriteInfo("Create country reports...");
                Parallel.ForEach(reportsBuilder.AvailableCountries.Select(reportsBuilder.GetCountryReport),
                    reportsSaver.WriteReport);

                logger.WriteInfo("Create country stats...");
                Parallel.ForEach(reportsBuilder.AvailableCountries.Select(reportsBuilder.GetCountryStats),
                    reportsSaver.WriteStats);
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
        }
    }
}