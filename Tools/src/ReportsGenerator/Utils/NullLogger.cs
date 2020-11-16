namespace ReportsGenerator.Utils
{
    /// <summary>
    /// Dummy logger implementation of the <see cref="ILogger"/> interface.
    /// </summary>
    public class NullLogger : ILogger
    {
        /// <inheritdoc />
        public int Indentation { get; } = 0;

        /// <inheritdoc />
        public void IndentDecrease()
        {
        }

        /// <inheritdoc />
        public void IndentIncrease()
        {
        }

        /// <inheritdoc />
        public void WriteError(string msg)
        {
        }

        /// <inheritdoc />
        public void WriteInfo(string msg)
        {
        }

        /// <inheritdoc />
        public void WriteWarning(string msg)
        {
        }
    }
}