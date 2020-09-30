using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ReportsGenerator.Data;
using ReportsGenerator.Data.Providers;
using ReportsGenerator.Model;
using ReportsGenerator.Model.Processors;
using ReportsGenerator.Utils;

namespace ReportsGenerator
{
    internal static class Program
    {
        private static void CreateCountryReports(Model.Reports.ReportsGenerator reportsGenerator)
        {
            var countriesMetrics = reportsGenerator.AvailableCountries.Select(reportsGenerator.GetCountryReport);

            Directory.CreateDirectory(Folders.CountriesReportsRoot);

            Parallel.ForEach(countriesMetrics, (countryMetrics) =>
            {
                var country = countryMetrics.Name;
                var dates = countryMetrics.AvailableDates.ToArray();
                Directory.CreateDirectory(Folders.GetCountryReportsFolder(country));
                var regionReportsPath = Folders.GetCountryRegionsFolder(country);
                if (countryMetrics.RegionReports.Any())
                    Directory.CreateDirectory(regionReportsPath);

                foreach (var regionMetrics in countryMetrics.RegionReports)
                {
                    var region = regionMetrics.Name.Replace('*', '_');
                    using var totalRegionFile = File.OpenWrite(Path.Combine(regionReportsPath, $"{region}.csv"));
                    using var totalRegionFileWriter = new StreamWriter(totalRegionFile);

                    totalRegionFileWriter.WriteLine(Consts.CountryReportHeader);

                    foreach (var day in dates)
                    {
                        var (confirmed, active, recovered, deaths) = regionMetrics.GetDayTotal(day);
                        var (confirmedChange, activeChange, recoveredChange, deathsChange) =
                            regionMetrics.GetDayChange(day);
                        var rt = regionMetrics.GetRt(day);
                        var ttr = regionMetrics.GetTimeToResolve(day);

                        totalRegionFileWriter.WriteLine(
                            $"{day:dd-MM-yyyy}," +
                            $"{confirmed},{active},{recovered},{deaths}," +
                            $"{confirmedChange},{activeChange},{recoveredChange},{deathsChange}," +
                            $"{rt.ToString("00.00000000", CultureInfo.InvariantCulture)}," +
                            $"{ttr:###########0}");
                    }
                }

                using var totalFile =
                    File.OpenWrite(Path.Combine(Folders.GetCountryReportsFolder(country), $"{country}.csv"));
                using var totalFileWriter = new StreamWriter(totalFile);
                totalFileWriter.WriteLine(Consts.CountryReportHeader);

                foreach (var day in dates)
                {
                    var (confirmed, active, recovered, deaths) = countryMetrics.GetDayTotal(day);
                    var (confirmedChange, activeChange, recoveredChange, deathsChange) =
                        countryMetrics.GetDayChange(day);
                    var rt = countryMetrics.GetRt(day);
                    var ttr = countryMetrics.GetTimeToResolve(day);

                    totalFileWriter.WriteLine(
                        $"{day:dd-MM-yyyy}," +
                        $"{confirmed},{active},{recovered},{deaths}," +
                        $"{confirmedChange},{activeChange},{recoveredChange},{deathsChange}," +
                        $"{rt.ToString("00.00000000", CultureInfo.InvariantCulture)}," +
                        $"{ttr:###########0}");
                }
            });
        }

        private static void CreateDayByDayReports(Model.Reports.ReportsGenerator reportsGenerator)
        {
            var dayByDayReports = reportsGenerator.AvailableDates.Select(reportsGenerator.GetDayReport);

            Directory.CreateDirectory(Folders.DayByDayReportsRoot);

            Parallel.ForEach(dayByDayReports, dayReport =>
            {
                var countries = dayReport.AvailableCountries.OrderBy(_ => _).ToArray();
                using var totalFile =
                    File.OpenWrite(Path.Combine(Folders.DayByDayReportsRoot, $"{dayReport.Day:yyyy-MM-dd}.csv"));
                using var totalFileWriter = new StreamWriter(totalFile);
                totalFileWriter.WriteLine(Consts.DayByDayReportHeader);

                foreach (var country in countries)
                {
                    var (confirmed, active, recovered, deaths) = dayReport.GetCountryTotal(country);
                    var (confirmedChange, activeChange, recoveredChange, deathsChange) =
                        dayReport.GetCountryChange(country);

                    totalFileWriter.WriteLine(
                        $"{country}," +
                        $"{confirmed},{active},{recovered},{deaths}," +
                        $"{confirmedChange},{activeChange},{recoveredChange},{deathsChange}");
                }
            });
        }

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

        private static bool IsInvalidData(Row row)
        {
            static bool IsInvalidDate(Row row, IEnumerable<string> badDates) =>
                badDates.Any(d => d == row[Field.LastUpdate]);

            return row[Field.CountryRegion] switch
            {
                "Ireland" when row[Field.LastUpdate] == "03-08-2020" => true,
                "The Gambia" => true,
                "The Bahamas" => true,
                "Republic of the Congo" => IsInvalidDate(row, Enumerable.Range(17, 5).Select(n => $"03-{n}-2020")),
                "Guam" => IsInvalidDate(row, Enumerable.Range(16, 6).Select(n => $"03-{n}-2020")),
                "Puerto Rico" => IsInvalidDate(row, Enumerable.Range(16, 6).Select(n => $"03-{n}-2020")),

                "Denmark" when row[Field.ProvinceState] == "Greenland" =>
                    IsInvalidDate(row, new[] { "03-19-2020", "03-20-2020", "03-21-2020" }),
                "Netherlands" when row[Field.ProvinceState] == "Aruba" =>
                    IsInvalidDate(row, new[] { "03-18-2020", "03-19-2020" }),

                "France" when row[Field.ProvinceState] == "Mayotte" => IsInvalidDate(row, Enumerable.Range(16, 6).Select(n => $"03-{n}-2020")),
                "France" when row[Field.ProvinceState] == "Guadeloupe" => IsInvalidDate(row, Enumerable.Range(16, 6).Select(n => $"03-{n}-2020")),
                "France" when row[Field.ProvinceState] == "Reunion" => IsInvalidDate(row, Enumerable.Range(16, 6).Select(n => $"03-{n}-2020")),
                "France" when row[Field.ProvinceState] == "French Guiana" => IsInvalidDate(row, Enumerable.Range(16, 6).Select(n => $"03-{n}-2020")),

                "Mainland China" => IsInvalidDate(row, new[] { "03-11-2020", "03-12-2020" }),

                "US" when row[Field.LastUpdate] == "03-22-2020" && row[Field.FIPS] == "11001" && row[Field.Deaths] == "0" => true,

                _ => false
            };
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

                void ReadFile(string filePath, bool fileNameIsDate)
                {
                    logger.WriteInfo($"--Processing file: {Path.GetFileName(filePath)}");
                    using var fs = File.OpenText(filePath);

                    foreach (var row in csvReader
                        .Read(fs, fileNameIsDate ? Path.GetFileNameWithoutExtension(filePath) : string.Empty))
                    {
                        if (IsInvalidData(row)) continue;

                        parsedData.Add(entryFactory.CreateEntry(row));
                    }
                }

                logger.WriteInfo("Reading raw data...");
                logger.IndentIncrease();
                Parallel.ForEach(Directory.EnumerateFiles(Folders.GetDataFolder<JHopkinsDataProvider>(), "*.csv"),
                    file => ReadFile(file, true));

                ReadFile(Path.Combine(Folders.GetDataFolder<YandexRussiaDataProvider>(), "Russia.csv"), false);

                logger.IndentDecrease();
                Folders.InitializeReportsFolders(argsParser);

                logger.WriteInfo("Initialize reports generator...");
                logger.IndentIncrease();

                var reportsGen = new Model.Reports.ReportsGenerator(logger);
                reportsGen.AddEntries(parsedData);

                logger.IndentDecrease();

                logger.WriteInfo("Create day by day reports...");
                CreateDayByDayReports(reportsGen);

                logger.WriteInfo("Create country reports...");
                CreateCountryReports(reportsGen);
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