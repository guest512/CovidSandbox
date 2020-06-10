using CovidSandbox.Data;
using CovidSandbox.Model;
using CovidSandbox.Model.Reports;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CovidSandbox
{
    internal static class Program
    {
        private static void Main()
        {
            var parsedData = new ConcurrentBag<Entry>();
            var csvReader = new CsvReader();

            void ReadFile(string filePath)
            {
                Debug.Assert(parsedData != null, nameof(parsedData) + " != null");
                Debug.Assert(csvReader != null, nameof(csvReader) + " != null");

                Console.WriteLine($"Processing file: {Path.GetFileName(filePath)}");
                using var fs = File.OpenText(filePath);

                foreach (var entry in csvReader.Read(fs).Select(_ => new Entry(_)))
                {
                    parsedData.Add(entry);
                }
            }

            //foreach (var filePath in Directory.EnumerateFiles("..\\..\\..\\Data\\JHopkins\\csse_covid_19_data\\csse_covid_19_daily_reports", "*.csv"))
            //{
            //    ReadFile(filePath);
            //}

            Parallel.ForEach(Directory.EnumerateFiles("Data\\JHopkins", "*.csv"),
            //new[]
            //{
            //    "C:\\Src\\Github\\CovidSandbox\\Data\\JHopkins\\csse_covid_19_data\\csse_covid_19_daily_reports\\01-22-2020.csv",
            //    "C:\\Src\\Github\\CovidSandbox\\Data\\JHopkins\\csse_covid_19_data\\csse_covid_19_daily_reports\\01-23-2020.csv",
            //    "C:\\Src\\Github\\CovidSandbox\\Data\\JHopkins\\csse_covid_19_data\\csse_covid_19_daily_reports\\01-24-2020.csv",
            //    "C:\\Src\\Github\\CovidSandbox\\Data\\JHopkins\\csse_covid_19_data\\csse_covid_19_daily_reports\\02-06-2020.csv"
            //},
            ReadFile);

            if (!Directory.Exists("reports"))
                Directory.CreateDirectory("reports");
            else if (Directory.EnumerateFileSystemEntries("reports").Any())
            {
                Directory.Delete("reports", true);
                Directory.CreateDirectory("reports");
                Thread.Sleep(100);
            }

            Console.WriteLine("Initialize reports generator...");
            var reportsGen = new ReportsGenerator();
            reportsGen.AddEntries(parsedData);

            Console.WriteLine("Create day by day reports...");
            CreateDayByDayReports(reportsGen);

            Console.WriteLine("Create country reports...");
            CreateCountryReports(reportsGen);
        }

        private static void CreateCountryReports(ReportsGenerator reportsGenerator)
        {
            var countriesMetrics = reportsGenerator.AvailableCountries.Select(reportsGenerator.GetCountryReport);

            Directory.CreateDirectory("reports\\countries");

            //foreach(var countryMetrics in countriesMetrics)
            Parallel.ForEach(countriesMetrics, (countryMetrics) =>
            {
                var country = countryMetrics.Name;
                var dates = countryMetrics.AvailableDates.ToArray();
                Directory.CreateDirectory($"reports\\countries\\{country}");
                if (countryMetrics.RegionReports.Any())
                    Directory.CreateDirectory($"reports\\countries\\{country}\\regions");

                foreach (var regionMetrics in countryMetrics.RegionReports)
                {
                    var region = regionMetrics.Name.Replace('*', '_');
                    using var totalRegionFile = File.OpenWrite($"reports\\countries\\{country}\\regions\\{region}.csv");
                    using var totalRegionFileWriter = new StreamWriter(totalRegionFile);

                    totalRegionFileWriter.WriteLine(
                    "Date, Confirmed, Active, Recovered, Deaths, Confirmed_Change, Active_Change, Recovered_Change, Deaths_Changge");

                    foreach (var day in dates)
                    {
                        var (confirmed, active, recovered, deaths) = regionMetrics.GetDayTotal(day);
                        var (confirmedChange, activeChange, recoveredChange, deathsChange) = regionMetrics.GetDayChange(day);

                        totalRegionFileWriter.WriteLine(
                            $"{day:dd-MM-yyyy}," +
                            $" {confirmed}, {active}, {recovered}, {deaths}," +
                            $" {confirmedChange}, {activeChange}, {recoveredChange}, {deathsChange}");
                    }
                }

                using var totalFile = File.OpenWrite($"reports\\countries\\{country}\\{country}.csv");
                using var totalFileWriter = new StreamWriter(totalFile);
                totalFileWriter.WriteLine(
                    "Date, Confirmed, Active, Recovered, Deaths, Confirmed_Change, Active_Change, Recovered_Change, Deaths_Change");

                foreach (var day in dates)
                {
                    var (confirmed, active, recovered, deaths) = countryMetrics.GetDayTotal(day);
                    var (confirmedChange, activeChange, recoveredChange, deathsChange) = countryMetrics.GetDayChange(day);

                    totalFileWriter.WriteLine(
                        $"{day:dd-MM-yyyy}," +
                        $" {confirmed}, {active}, {recovered}, {deaths}," +
                        $" {confirmedChange}, {activeChange}, {recoveredChange}, {deathsChange}");
                }
            });
        }

        private static void CreateDayByDayReports(ReportsGenerator reportsGenerator)
        {
            var dayByDayReports = reportsGenerator.AvailableDates.Select(reportsGenerator.GetDayReport);

            Directory.CreateDirectory("reports\\dayByDay");

            //foreach (var dayReport in dayByDayReports)
            Parallel.ForEach(dayByDayReports, dayReport =>
            {
                var countries = dayReport.AvailableCountries.OrderBy(_=>_).ToArray();
                using var totalFile = File.OpenWrite($"reports\\dayByDay\\{dayReport.Day:yyyy-MM-dd}.csv");
                using var totalFileWriter = new StreamWriter(totalFile);
                totalFileWriter.WriteLine(
                    "CountryRegion, Confirmed, Active, Recovered, Deaths, Confirmed_Change, Active_Change, Recovered_Change, Deaths_Change");

                foreach (var country in countries)
                {
                    var (confirmed, active, recovered, deaths) = dayReport.GetCountryTotal(country);
                    var (confirmedChange, activeChange, recoveredChange, deathsChange) =
                        dayReport.GetCountryChange(country);

                    totalFileWriter.WriteLine(
                        $"{country}," +
                        $" {confirmed}, {active}, {recovered}, {deaths}," +
                        $" {confirmedChange}, {activeChange}, {recoveredChange}, {deathsChange}");
                }
            });
        }
    }
}