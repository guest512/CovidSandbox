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

            throw new ArgumentOutOfRangeException(nameof(T));
        }

        private static Dictionary<RowVersion, IRowProcessor> GetRowProcessors(ILogger logger)
        {
            var jHopkinsProcessor = new JHopkinsRowProcessor(logger);

            var rowProcessors = new Dictionary<RowVersion, IRowProcessor>
            {
                {RowVersion.JHopkinsV1, jHopkinsProcessor},
                {RowVersion.JHopkinsV2, jHopkinsProcessor},
                {RowVersion.JHopkinsV3, jHopkinsProcessor},
                {RowVersion.JHopkinsV4, jHopkinsProcessor},
                {RowVersion.YandexRussia, new YandexRussiaRowProcessor(logger)}
            };

            return rowProcessors;
        }

        private static async Task Main(string[] args)
        {
            var logger = new ConsoleLogger();
            try
            {
                logger.WriteInfo("Logger created");

                var argsParser = new ArgsParser(args, logger);

                var parsedData = new List<Entry>();

                var entryFactory = new EntryFactory(GetRowProcessors(logger), logger);
                var reportsSaver = new ReportsSaver(new StringReportFormatter(), new CsvFileReportStorage(argsParser.ReportsDir, true), new NullLogger());
                var reportsGen = new Model.Reports.ReportsGenerator(logger);

                var dataSources = new IDataSource[]
                {
                    new JHopkinsDataSource(GetDataFolder<JHopkinsDataSource>(), logger),
                    new YandexRussiaDataSource(GetDataFolder<YandexRussiaDataSource>(), logger)
                };

                logger.WriteInfo("Reading raw data...");
                logger.IndentIncrease();

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

                reportsGen.AddEntries(parsedData);

                logger.IndentDecrease();

                logger.WriteInfo("Create day by day reports...");
                Parallel.ForEach(reportsGen.AvailableDates.Select(reportsGen.GetDayReport),
                    reportsSaver.WriteReport);

                logger.WriteInfo("Create country reports...");
                Parallel.ForEach(reportsGen.AvailableCountries.Select(reportsGen.GetCountryReport),
                    reportsSaver.WriteReport);
            }
            catch (Exception ex)
            {
                while (logger.Indentation > 0)
                {
                    logger.IndentDecrease();
                }

                logger.WriteError(ex.ToString());
            }
            finally
            {
                logger.Dispose();
            }
        }
    }
}