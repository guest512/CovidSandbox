using ReportsGenerator.Data;

namespace ReportsGenerator.Model
{
    public interface IStatsProvider
    {
        string GetStatsName(Row row);

        string GetCountryStatsName(string countryName);

        string LookupContinentName(string statsName);

        long LookupPopulation(string statsName);
    }
}