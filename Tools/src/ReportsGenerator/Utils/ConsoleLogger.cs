using System;
using System.Linq;
using System.Threading;

namespace ReportsGenerator.Utils
{
    public class ConsoleLogger : ILogger, IDisposable
    {
        private const int IndentationLength = 2;
        private const char IndentationSymbol = ' ';
        private const int MaxIndentation = 20;
        private readonly SemaphoreSlim _locker = new SemaphoreSlim(1, 1);

        public int Indentation { get; private set; }

        public void Dispose()
        {
            _locker.Dispose();
        }

        public void IndentDecrease()
        {
            RunUnderWaitEvent(() =>
                {
                    if (Indentation > 0)
                        Indentation--;
                });
        }

        public void IndentIncrease()
        {
            RunUnderWaitEvent(() =>
            {
                if (Indentation < MaxIndentation)
                    Indentation++;
            });
        }

        public void WriteError(string msg)
        {
            WriteMessage(msg, ConsoleColor.DarkRed);
        }

        public void WriteInfo(string msg)
        {
            WriteMessage(msg, ConsoleColor.DarkGray);
        }

        public void WriteWarning(string msg)
        {
            WriteMessage(msg, ConsoleColor.DarkYellow);
        }

        private void WriteMessage(string msg, ConsoleColor foregroundColor)
        {
            var origColor = Console.ForegroundColor;
            Console.ForegroundColor = foregroundColor;

            var messageText =
                $"{new string(Enumerable.Repeat(IndentationSymbol, IndentationLength * Indentation).ToArray())}{msg}";

            Console.WriteLine(messageText);

            Console.ForegroundColor = origColor;
        }

        private void RunUnderWaitEvent(Action action)
        {
            _locker.Wait();
            action();
            _locker.Release();
        }
    }
}