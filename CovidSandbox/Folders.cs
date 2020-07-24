using CovidSandbox.Data.Providers;
using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace CovidSandbox
{
    public static class Folders
    {
        public const string CountriesReportsRoot = "reports\\countries";
        public const string DayByDayReportsRoot = "reports\\dayByDay";
        public const string ReportsRoot = "reports";
        private const string DataRoot = "Data";

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

        public static void InitializeReportsFolders()
        {
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