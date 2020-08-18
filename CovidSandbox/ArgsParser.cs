using System;
using System.Collections.Generic;

namespace CovidSandbox
{
    public class ArgsParser
    {
        public string ReportsDir { get; private set; } = string.Empty;
        public ArgsParser(IEnumerable<string> args)
        {
            foreach(var arg in args)
            {
                ParseArgs(arg);
            }
        }
        public void ParseArgs(string argKVPair)
        {
            var argNameValue  = argKVPair.Split(":");
            switch(argNameValue[0])
            {
                case "reportsDir":
                    ReportsDir = argNameValue[1];
                    break;

                default:
                    Console.WriteLine($"Unknown argument: {argKVPair}");
                    break;
            }
        }
    }
}