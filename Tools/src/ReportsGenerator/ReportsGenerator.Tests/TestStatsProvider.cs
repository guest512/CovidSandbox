using ReportsGenerator.Data;
using ReportsGenerator.Model;

namespace ReportsGenerator.Tests
{
    public class TestStatsProvider : IStatsProvider
    {
        public string GetStatsName(Row row) => "TEST NAME";

        public string GetCountryStatsName(string countryName) => countryName;

        public string LookupContinentName(string statsName) => "TEST";

        public long LookupPopulation(string statsName) => -1;
    }
}