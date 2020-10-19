using ReportsGenerator.Data;
using System;

namespace ReportsGenerator.Model.Processors
{
    public interface IRowProcessor
    {
        long GetActive(Row row);

        long GetConfirmed(Row row);

        string GetCountryName(Row row);

        string GetCountyName(Row row);

        long GetDeaths(Row row);

        IsoLevel GetIsoLevel(Row row);

        DateTime GetLastUpdate(Row row);

        Origin GetOrigin(Row row);

        string GetProvinceName(Row row);

        long GetRecovered(Row row);

        string GetStatsName(Row row);
    }
}