using ReportsGenerator.Data;
using ReportsGenerator.Model;

namespace ReportsGenerator.Tests
{
    public class TestStatsProvider : IStatsProvider
    {
        public string GetStatsName(Row row)
        {
            return "TEST NAME";
        }

        public string GetCountryStatsName(string countryName)
        {
            return countryName;
        }

        public string LookupContinentName(string statsName)
        {
            return "TEST";
        }

        public long LookupPopulation(string statsName)
        {
            return -1;
        }
    }
}