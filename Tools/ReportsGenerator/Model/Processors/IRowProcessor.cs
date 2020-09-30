using CovidSandbox.Data;
using System;

namespace CovidSandbox.Model.Processors
{
    public interface IRowProcessor
    {
        long GetActive(Row row);

        long GetConfirmed(Row row);

        string GetCountryName(Row row);

        string GetCountyName(Row row);

        long GetDeaths(Row row);

        uint GetFips(Row row);

        IsoLevel GetIsoLevel(Row row);

        DateTime GetLastUpdate(Row row);

        Origin GetOrigin(Row row);

        string GetProvinceName(Row row);

        long GetRecovered(Row row);
    }
}