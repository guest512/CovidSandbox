namespace CovidSandbox.Utils
{
    public interface ILogger
    {
        public int Indentation { get; }

        public void IndentDecrease();

        public void IndentIncrease();

        public void WriteError(string msg);

        public void WriteInfo(string msg);

        public void WriteWarning(string msg);
    }
}