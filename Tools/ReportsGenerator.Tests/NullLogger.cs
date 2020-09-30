using System;
using System.Collections.Generic;
using System.Text;
using CovidSandbox.Utils;

namespace CovidSandbox.Tests
{
    class NullLogger : ILogger
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
