using System;
using System.Collections.Generic;
using CovidSandbox.Utils;

namespace CovidSandbox
{
    public class ArgsParser
    {
        private readonly ILogger _logger;
        public string ReportsDir { get; private set; } = string.Empty;
        public ArgsParser(IEnumerable<string> args, ILogger logger)
        {
            _logger = logger;
            _logger.WriteInfo("Parsing arguments...");
            _logger.IndentIncrease();
            foreach(var arg in args)
            {
                ParseArgs(arg);
            }
            _logger.IndentDecrease();
        }
        public void ParseArgs(string argKVPair)
        {
            _logger.WriteInfo($"--Parsing '{argKVPair}' argument");
            var argNameValue  = argKVPair.Split(":");
            switch(argNameValue[0])
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