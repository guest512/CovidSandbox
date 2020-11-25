namespace ReportsGenerator.Utils
{
    /// <summary>
    /// Represents a logger interface.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Gets a current indentation level.
        /// </summary>
        public int Indentation { get; }

        /// <summary>
        /// Increases indentation level.
        /// </summary>
        public void IndentDecrease();

        /// <summary>
        /// Decreases indentation level.
        /// </summary>
        public void IndentIncrease();

        /// <summary>
        /// Writes an error message with current indentation level.
        /// </summary>
        /// <param name="msg">Message to write.</param>
        public void WriteError(string msg);

        /// <summary>
        /// Writes an informational message with current indentation level.
        /// </summary>
        /// <param name="msg">Message to write.</param>
        public void WriteInfo(string msg);

        /// <summary>
        /// Writes a warning message with current indentation level.
        /// </summary>
        /// <param name="msg">Message to write.</param>
        public void WriteWarning(string msg);
    }
}