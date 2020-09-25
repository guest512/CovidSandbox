using CovidSandbox.Data;
using CovidSandbox.Data.Providers;
using CovidSandbox.Model;
using CovidSandbox.Model.Reports;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CovidSandbox
{
    internal static class Program
    {
        private static void CreateCountryReports(ReportsGenerator reportsGenerator)
        {
            var countriesMetrics = reportsGenerator.AvailableCountries.Select(reportsGenerator.GetCountryReport);

            Directory.CreateDirectory(Folders.CountriesReportsRoot);

            //foreach(var countryMetrics in countriesMetrics)
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

                        totalRegionFileWriter.WriteLine(
                            $"{day:dd-MM-yyyy}," +
                            $"{confirmed},{active},{recovered},{deaths}," +
                            $"{confirmedChange},{activeChange},{recoveredChange},{deathsChange}");
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

                    totalFileWriter.WriteLine(
                        $"{day:dd-MM-yyyy}," +
                        $"{confirmed},{active},{recovered},{deaths}," +
                        $"{confirmedChange},{activeChange},{recoveredChange},{deathsChange}");
                }
            });
        }

        private static void CreateDayByDayReports(ReportsGenerator reportsGenerator)
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

        private static void Main(string[] args)
        {
            var argsParser = new ArgsParser(args);
            var parsedData = new ConcurrentBag<Entry>();
            var csvReader = new CsvReader();
            var entryFactory = new EntryFactory();

            void ReadFile(string filePath, bool fileNameIsDate)
            {
                Console.WriteLine($"Processing file: {Path.GetFileName(filePath)}");
                using var fs = File.OpenText(filePath);

                foreach (var entry in csvReader
                    .Read(fs, fileNameIsDate ? Path.GetFileNameWithoutExtension(filePath) : string.Empty)
                    .Select(_ => entryFactory.CreateEntry(_)))
                {
                    parsedData.Add(entry);
                }
            }

            Parallel.ForEach(Directory.EnumerateFiles(Folders.GetDataFolder<JHopkinsDataProvider>(), "*.csv"),
                file => ReadFile(file, true));

            ReadFile(Path.Combine(Folders.GetDataFolder<YandexRussiaDataProvider>(), "Russia.csv"), false);

            Folders.InitializeReportsFolders(argsParser);

            Console.WriteLine("Initialize reports generator...");
            var reportsGen = new ReportsGenerator();
            reportsGen.AddEntries(parsedData);

            Console.WriteLine("Create day by day reports...");
            CreateDayByDayReports(reportsGen);

            Console.WriteLine("Create country reports...");
            CreateCountryReports(reportsGen);
        }
    }
}