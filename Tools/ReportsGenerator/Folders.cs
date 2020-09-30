using System;
using System.IO;
using System.Linq;
using System.Threading;
using ReportsGenerator.Data.Providers;

namespace ReportsGenerator
{
    public static class Folders
    {
        private const string DataRoot = "Data";

        static Folders()
        {
            InitializeReportsFolders("reports");
        }

        public static string CountriesReportsRoot { get; private set; } = string.Empty;
        public static string DayByDayReportsRoot { get; private set; } = string.Empty;
        public static string ReportsRoot { get; private set; } = string.Empty;

        public static string GetCountryRegionsFolder(string countryName) =>
            Path.Combine(CountriesReportsRoot, countryName, "regions");

        public static string GetCountryReportsFolder(string countryName) =>
            Path.Combine(CountriesReportsRoot, countryName);

        public static string GetDataFolder<T>() where T : IDataProvider
        {
            if (typeof(T) == typeof(JHopkinsDataProvider))
            {
                return Path.Combine(DataRoot, "JHopkins");
            }

            if (typeof(T) == typeof(YandexRussiaDataProvider))
            {
                return Path.Combine(DataRoot, "Yandex");
            }

            throw new ArgumentOutOfRangeException(nameof(T));
        }

        public static void InitializeReportsFolders(ArgsParser argsParser)
        {
            if (!string.IsNullOrWhiteSpace(argsParser.ReportsDir))
                InitializeReportsFolders(argsParser.ReportsDir);
        }

        private static void InitializeReportsFolders(string reportsRoot)
        {
            ReportsRoot = reportsRoot;
            CountriesReportsRoot = Path.Combine(ReportsRoot, "countries");
            DayByDayReportsRoot = Path.Combine(ReportsRoot, "dayByDay");

            if (!Directory.Exists(ReportsRoot))
                Directory.CreateDirectory(ReportsRoot);
            else if (Directory.EnumerateFileSystemEntries(ReportsRoot).Any())
            {
                Directory.Delete(ReportsRoot, true);
                Directory.CreateDirectory(ReportsRoot);
                Thread.Sleep(100);
            }
        }
    }
}