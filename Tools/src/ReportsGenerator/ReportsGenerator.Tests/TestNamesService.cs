using ReportsGenerator.Model;

namespace ReportsGenerator.Tests
{
    public class TestNamesService : INames
    {
        public string GetCyrillicName(string latinName)
        {
            if (latinName == "Kamchatka Krai")
                return "TEST CYRILLIC NAME";

            return latinName;
        }

        public string GetLatinName(string cyrillicName)
        {
            return cyrillicName;
        }

        public string GetStateFullName(string stateAbbrev)
        {
            return stateAbbrev;
        }
    }
}