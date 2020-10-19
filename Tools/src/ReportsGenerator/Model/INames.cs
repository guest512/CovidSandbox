namespace ReportsGenerator.Model
{
    public interface INames
    {
        string GetCyrillicName(string latinName);

        string GetLatinName(string cyrillicName);

        string GetStateFullName(string stateAbbrev);
    }
}