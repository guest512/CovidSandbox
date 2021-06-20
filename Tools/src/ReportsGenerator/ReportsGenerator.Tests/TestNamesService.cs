using ReportsGenerator.Model;

namespace ReportsGenerator.Tests
{
    public class TestNamesService : INames
    {
        public string GetCyrillicName(string latinName) => latinName == "Kamchatka Krai" ? "TEST CYRILLIC NAME" : latinName;

        public string GetLatinName(string cyrillicName) => cyrillicName;

        public string GetStateFullName(string stateAbbrev) => stateAbbrev;
    }
}