using ReportsGenerator.Data;
using ReportsGenerator.Data.DataSources;
using ReportsGenerator.Data.DataSources.Providers;
using ReportsGenerator.Data.IO;
using ReportsGenerator.Model;
using ReportsGenerator.Model.Processors;
using ReportsGenerator.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ReportsGenerator
{
    internal static class Program
    {
        private static Dictionary<RowVersion, IDataProvider> GetDataProviders()
        {
            var jHopkinsProvider = new JHopkinsDataProvider();
            var dataProviders = new Dictionary<RowVersion, IDataProvider>
            {
                {RowVersion.JHopkinsV1, jHopkinsProvider},
                {RowVersion.JHopkinsV2, jHopkinsProvider},
                {RowVersion.JHopkinsV3, jHopkinsProvider},
                {RowVersion.JHopkinsV4, jHopkinsProvider},
                {RowVersion.YandexRussia, new YandexRussiaDataProvider()}
            };
            return dataProviders;
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

        private static void Main(string[] args)
        {
            var logger = new ConsoleLogger();
            try
            {
                logger.WriteInfo("Logger created");

                var argsParser = new ArgsParser(args, logger);

                var dataProviders = GetDataProviders();
                var rowProcessors = GetRowProcessors(logger);

                var parsedData = new ConcurrentBag<Entry>();
                var csvReader = new CsvReader(dataProviders, logger);
                var entryFactory = new EntryFactory(rowProcessors, logger);

                var reportsSaver = new ReportsSaver(new StringReportFormatter(), new CsvFileReportStorage(argsParser.ReportsDir, true), new NullLogger());

                logger.WriteInfo("Reading raw data...");
                logger.IndentIncrease();
                Parallel.ForEach(Directory.EnumerateFiles(Folders.GetDataFolder<JHopkinsDataProvider>(), "*.csv"),
                    file => ReadFile(file, true, logger, csvReader, parsedData, entryFactory));

                ReadFile(Path.Combine(Folders.GetDataFolder<YandexRussiaDataProvider>(), "Russia.csv"), false, logger, csvReader, parsedData, entryFactory);

                logger.IndentDecrease();

                logger.WriteInfo("Initialize reports generator...");
                logger.IndentIncrease();

                var reportsGen = new Model.Reports.ReportsGenerator(logger);
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

        private static void ReadFile(string filePath, bool fileNameIsDate, ILogger logger, CsvReader csvReader, ConcurrentBag<Entry> parsedData, EntryFactory entryFactory)
        {
            logger.WriteInfo($"--Processing file: {Path.GetFileName(filePath)}");
            using var fs = File.OpenText(filePath);

            foreach (var row in csvReader.Read(fs, fileNameIsDate ? Path.GetFileNameWithoutExtension(filePath) : string.Empty))
            {
                parsedData.Add(entryFactory.CreateEntry(row));
            }
        }
    }
}