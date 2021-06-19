using System.Collections.Generic;
using ReportsGenerator.Utils;

namespace ReportsGenerator
{
    /// <summary>
    /// Command-line arguments parser.
    /// </summary>
    public class ArgsParser
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArgsParser"/> class.
        /// </summary>
        /// <param name="args">Command line arguments collection.</param>
        /// <param name="logger">A <see cref="ILogger"/> instance.</param>
        public ArgsParser(IEnumerable<string> args, ILogger logger)
        {
            _logger = logger;
            _logger.WriteInfo("Parsing arguments...");
            _logger.IndentIncrease();
            foreach (var arg in args)
            {
                ParseArgs(arg);
            }
            _logger.IndentDecrease();
        }

        /// <summary>
        /// Gets a directory where results should be saved.
        /// </summary>
        public string ReportsDir { get; private set; } = string.Empty;

        // ReSharper disable once InconsistentNaming
        private void ParseArgs(string argKVPair)
        {
            _logger.WriteInfo($"--Parsing '{argKVPair}' argument");
            var argNameValue = argKVPair.Split(":");
            switch (argNameValue[0])
            {
                case "reportsDir":
                    ReportsDir = argNameValue[1];
                    break;

                default:
                    _logger.WriteWarning($"Unknown argument: {argKVPair}");
                    break;
            }
        }
    }
}