using CovidSandbox.Data;
using CovidSandbox.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace CovidSandbox
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var parsedData = new List<Entry>();
            foreach (var filePath in Directory.EnumerateFiles("C:\\Src\\Github\\COVID-19\\csse_covid_19_data\\csse_covid_19_daily_reports", "*.csv"))
            {
                using var fs = File.OpenText(filePath);
                var headersCount = Utils.SplitCsvRowString(fs.ReadLine()).Length;
                var version = headersCount switch
                {
                    6 => RowVersion.V1,
                    8 => RowVersion.V2,
                    12 => RowVersion.V3,
                    _ => throw new Exception($"File {filePath} has unknown format")
                };
                while (!fs.EndOfStream)
                {
                    var line = fs.ReadLine();
                    parsedData.Add(new Entry(new Row(line, version)));
                }
            }

            if (!Directory.Exists("output"))
                Directory.CreateDirectory("output");
            else if (Directory.EnumerateFileSystemEntries("output").Any())
            {
                Directory.Delete("output", true);
                Directory.CreateDirectory("output");
                Thread.Sleep(100);
            }

            CreateDayByDayReports(parsedData);
            CreateCountryReports(parsedData);
        }

        private static void CreateCountryReports(IEnumerable<Entry> parsedData)
        {
            var countriesData = parsedData.GroupBy(_ => _.CountryRegion);
            var countriesMetrics =
                new List<(string Country, List<(DateTime Day, (int Confirmed, int Active, int Recovered, int Deaths)
                    DayMetrics)>)>();

            foreach (var countryData in countriesData)
            {
                var dayByDayMetrics =
                    new List<(DateTime Day, (int Confirmed, int Active, int Recovered, int Deaths) DayMetrics)>();
                var dayByDayData = countryData.GroupBy(_ => _.LastUpdate);
                foreach (var dayData in dayByDayData)
                {
                    var dayMetrics = GetMetrics(dayData);
                    dayByDayMetrics.Add((dayData.Key, dayMetrics));
                }

                countriesMetrics.Add((countryData.Key, dayByDayMetrics));
            }

            Directory.CreateDirectory("output\\countries");

            foreach (var (country, metrics) in countriesMetrics)
            {
                using var totalFile = File.OpenWrite($"output\\countries\\{country}.csv");
                using var totalFileWriter = new StreamWriter(totalFile);
                totalFileWriter.WriteLine("Date, Confirmed, Active, Recovered, Deaths, Confirmed_Change, Active_Change, Recovered_Change, Deaths_Changge");

                for (var i = 0; i < metrics.Count; i++)
                {
                    var (day, (confirmed, active, recovered, deaths)) = metrics[i];
                    var (_, (prevConfirmed, prevActive, prevRecovered, prevDeaths)) =
                        i > 0 ? metrics[i - 1] : (DateTime.MinValue, (0, 0, 0, 0));

                    totalFileWriter.WriteLine($"{day:dd-MM-yyyy}, " +
                                              $"{confirmed}, " +
                                              $"{active}, " +
                                              $"{recovered}, " +
                                              $"{deaths}, " +
                                              $"{confirmed - prevConfirmed}, " +
                                              $"{active - prevActive}, " +
                                              $"{recovered - prevRecovered}, " +
                                              $"{deaths - prevDeaths}");
                }
            }

        }

        private static void CreateDayByDayReports(IEnumerable<Entry> parsedData)
        {
            var dayByDayData = parsedData.GroupBy(_ => _.LastUpdate);
            var dayByDayMetrics =
                new List<(DateTime day,
                    List<(string CountryName, (int Confirmed, int Active, int Recovered, int Deaths) CountryMetrics)> dayMetrics
                    )>();

            foreach (var dayData in dayByDayData)
            {
                var dayMetrics =
                    new List<(string countryName, (int Confirmed, int Active, int Recovered, int Deaths) CountryMetrics)>();
                var countriesData = dayData.GroupBy(_ => _.CountryRegion);
                foreach (var countryData in countriesData)
                {
                    var countryMetrics = GetMetrics(countryData);
                    dayMetrics.Add((countryData.Key, countryMetrics));
                }

                dayByDayMetrics.Add((dayData.Key, dayMetrics));
            }

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
                                                 $"{currentCountryMetrics.Confirmed - previousCountryMetrics.Confirmed}, " +
                                                 $"{currentCountryMetrics.Active - previousCountryMetrics.Active}, " +
                                                 $"{currentCountryMetrics.Recovered - previousCountryMetrics.Recovered}, " +
                                                 $"{currentCountryMetrics.Deaths - previousCountryMetrics.Deaths}");

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

        private static (int Confirmed, int Active, int Recovered, int Deaths) GetMetrics(IEnumerable<Entry> grpByCountry)
        {
            var confirmed = 0;
            var active = 0;
            var recovered = 0;
            var deaths = 0;
            foreach (var entry in grpByCountry)
            {
                confirmed += entry.Confirmed.GetValueOrDefault();
                active += entry.Active.GetValueOrDefault();
                recovered += entry.Recovered.GetValueOrDefault();
                deaths += entry.Deaths.GetValueOrDefault();
            }

            return (confirmed, active, recovered, deaths);
        }
    }
}