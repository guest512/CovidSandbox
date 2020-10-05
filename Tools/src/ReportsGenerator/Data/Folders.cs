using System;
using System.IO;
using ReportsGenerator.Data.DataSources.Providers;

namespace ReportsGenerator.Data
{
    public static class Folders
    {
        private const string DataRoot = "Data";

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
    }
}