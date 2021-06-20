namespace ReportsGenerator.Utils
{
    /// <summary>
    /// Dummy logger implementation of the <see cref="ILogger"/> interface.
    /// </summary>
    public class NullLogger : ILogger
    {
        /// <inheritdoc />
        public int Indentation => 0;

        /// <inheritdoc />
        public void IndentDecrease()
        {
            //NOTHING SHOULD HAPPEN HERE
        }

        /// <inheritdoc />
        public void IndentIncrease()
        {
            //NOTHING SHOULD HAPPEN HERE
        }

        /// <inheritdoc />
        public void WriteError(string msg)
        {
            //NOTHING SHOULD HAPPEN HERE
        }

        /// <inheritdoc />
        public void WriteInfo(string msg)
        {
            //NOTHING SHOULD HAPPEN HERE
        }

        /// <inheritdoc />
        public void WriteWarning(string msg)
        {
            //NOTHING SHOULD HAPPEN HERE
        }
    }
}