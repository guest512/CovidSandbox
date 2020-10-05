namespace ReportsGenerator.Utils
{
    public class NullLogger : ILogger
    {
        public int Indentation { get; } = 0;

        public void IndentDecrease()
        {
        }

        public void IndentIncrease()
        {
        }

        public void WriteError(string msg)
        {
        }

        public void WriteInfo(string msg)
        {
        }

        public void WriteWarning(string msg)
        {
        }
    }
}