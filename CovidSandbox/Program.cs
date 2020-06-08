using CovidSandbox.Data;
using CovidSandbox.Model;
using CovidSandbox.Model.Reports;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

            Parallel.ForEach(Directory.EnumerateFiles("..\\..\\..\\Data\\JHopkins\\csse_covid_19_data\\csse_covid_19_daily_reports", "*.csv"),
            ReadFile);

            if (!Directory.Exists("output"))
                Directory.CreateDirectory("output");
            else if (Directory.EnumerateFileSystemEntries("output").Any())
            {
                Directory.Delete("output", true);
                Directory.CreateDirectory("output");
                Thread.Sleep(100);
            }

            Console.WriteLine("Create day by day reports...");
            CreateDayByDayReports(parsedData.OrderBy(_ => _.LastUpdate));

            Console.WriteLine("Create country reports...");
            CreateCountryReports(parsedData.OrderBy(_ => _.LastUpdate));
        }

        private static void CreateCountryReports(IEnumerable<Entry> parsedData)
        {
            var countriesData = parsedData.GroupBy(_ => _.CountryRegion);
            var countriesMetrics = countriesData.Select(_ => new CountryReport(_.Key, _));

            Directory.CreateDirectory("output\\countries");

            //foreach(var countryMetrics in countriesMetrics)
            Parallel.ForEach(countriesMetrics, (countryMetrics) =>
            {
                var country = countryMetrics.Name;
                var dates = countryMetrics.GetAvailableDates().ToArray();
                Directory.CreateDirectory($"output\\countries\\{country}");
                if (countryMetrics.RegionReports.Any(_ => !string.IsNullOrEmpty(_.Name)))
                    Directory.CreateDirectory($"output\\countries\\{country}\\regions");

                foreach (var regionMetrics in countryMetrics.RegionReports.Where(_ => !string.IsNullOrEmpty(_.Name)))
                {
                    var region = regionMetrics.Name.Replace('*', '_');
                    using var totalRegionFile = File.OpenWrite($"output\\countries\\{country}\\regions\\{region}.csv");
                    using var totalRegionFileWriter = new StreamWriter(totalRegionFile);

                    totalRegionFileWriter.WriteLine(
                    "Date, Confirmed, Active, Recovered, Deaths, Confirmed_Change, Active_Change, Recovered_Change, Deaths_Changge");

                    foreach (var day in dates)
                    {
                        var (confirmed, active, recovered, deaths) = regionMetrics.GetTotalByDay(day);
                        var (prevConfirmed, prevActive, prevRecovered, prevDeaths) = regionMetrics.GetTotalByDay(day.AddDays(-1).Date);

                        totalRegionFileWriter.WriteLine($"{day:dd-MM-yyyy}, {confirmed}, {active}, {recovered}, {deaths}, "
                        + $"{confirmed - (int)prevConfirmed}, "
                        + $"{active - (int)prevActive}, "
                        + $"{recovered - (int)prevRecovered}, "
                        + $"{deaths - (int)prevDeaths}");
                    }
                }

                using var totalFile = File.OpenWrite($"output\\countries\\{country}\\{country}.csv");
                using var totalFileWriter = new StreamWriter(totalFile);
                totalFileWriter.WriteLine(
                    "Date, Confirmed, Active, Recovered, Deaths, Confirmed_Change, Active_Change, Recovered_Change, Deaths_Changge");

                foreach (var day in dates)
                {
                    var (confirmed, active, recovered, deaths) = countryMetrics.GetTotalByDay(day);
                    var (prevConfirmed, prevActive, prevRecovered, prevDeaths) = countryMetrics.GetTotalByDay(day.AddDays(-1).Date);

                    totalFileWriter.WriteLine($"{day:dd-MM-yyyy}, {confirmed}, {active}, {recovered}, {deaths}, "
                        + $"{confirmed - (int)prevConfirmed}, "
                        + $"{active - (int)prevActive}, "
                        + $"{recovered - (int)prevRecovered}, "
                        + $"{deaths - (int)prevDeaths}");
                }
            });
        }

        private static void CreateDayByDayReports(IEnumerable<Entry> parsedData)
        {
            var dayByDayData = parsedData.GroupBy(_ => _.LastUpdate);
            var dayByDayMetrics = (from dayData in dayByDayData
                                   let countriesData = dayData.GroupBy(_ => _.CountryRegion)
                                   let dayMetrics =
                                       (from countryData in countriesData
                                        let countryMetrics = GetMetrics(countryData)
                                        select (CountryName: countryData.Key, countryMetrics)).ToList()
                                   select (Date: dayData.Key, dayMetrics)).ToList();

            Directory.CreateDirectory("output\\total");
            Directory.CreateDirectory("output\\changes");

            var countriesList = new string[0];
            var (_, previousMetrics) = dayByDayMetrics[0];

            for (var i = 0; i < dayByDayMetrics.Count; i++)
            {
                var (day, currentMetrics) = dayByDayMetrics[i];

                using var totalFile = File.OpenWrite($"output\\total\\{day:yyyy-MM-dd}.csv");
                using var totalFileWriter = new StreamWriter(totalFile);
                totalFileWriter.WriteLine("CountryRegion, Confirmed, Active, Recovered, Deaths");
                foreach (var (countryName, (confirmed, active, recovered, deaths)) in currentMetrics.OrderBy(_ => _.CountryName)
                )
                {
                    totalFileWriter.WriteLine($"{countryName.ToCsvString()}, " +
                                              $"{confirmed}, " +
                                              $"{active}, " +
                                              $"{recovered}, " +
                                              $"{deaths}");
                }

                countriesList = countriesList.Union(currentMetrics.Select(_ => _.CountryName)).OrderBy(_ => _).ToArray();

                if (i == 0)
                    continue;

                using var incrFile = File.OpenWrite($"output\\changes\\{day:yyyy-MM-dd}.csv");
                using var incrFileWriter = new StreamWriter(incrFile);
                incrFileWriter.WriteLine("CountryRegion, Confirmed, Active, Recovered, Deaths");

                foreach (var country in countriesList)
                {
                    var (_, currentCountryMetrics) = currentMetrics.FirstOrDefault(_ => _.CountryName == country);
                    var (_, previousCountryMetrics) = previousMetrics.FirstOrDefault(_ => _.CountryName == country);

                    if (currentMetrics.All(_ => _.CountryName != country))
                    {
                        incrFileWriter.WriteLine($"{country.ToCsvString()}, " +
                                                 "0, " +
                                                 "0, " +
                                                 "0, " +
                                                 "0");
                    }
                    else
                    {
                        incrFileWriter.WriteLine($"{country.ToCsvString()}, " +
                                                 $"{currentCountryMetrics.Confirmed - (int)previousCountryMetrics.Confirmed}, " +
                                                 $"{currentCountryMetrics.Active - (int)previousCountryMetrics.Active}, " +
                                                 $"{currentCountryMetrics.Recovered - (int)previousCountryMetrics.Recovered}, " +
                                                 $"{currentCountryMetrics.Deaths - (int)previousCountryMetrics.Deaths}");

                        var idx = previousMetrics.IndexOf((country, previousCountryMetrics));
                        if (idx != -1)
                        {
                            previousMetrics[idx] = (country, currentCountryMetrics);
                        }
                        else
                        {
                            previousMetrics.Add((country, currentCountryMetrics));
                        }
                    }
                }
            }
        }

        private static Metrics GetMetrics(IEnumerable<Entry> grpByCountry)
        {
            uint confirmed = 0,
                active = 0,
                recovered = 0,
                deaths = 0;

            foreach (var entry in grpByCountry)
            {
                confirmed += entry.Confirmed;
                active += entry.Active;
                recovered += entry.Recovered;
                deaths += entry.Deaths;
            }

            return new Metrics(confirmed, active, recovered, deaths);
        }
    }
}