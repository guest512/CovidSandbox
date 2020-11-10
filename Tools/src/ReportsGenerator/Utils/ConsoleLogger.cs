using System;
using System.Linq;
using System.Threading;

namespace ReportsGenerator.Utils
{
    /// <summary>
    /// Represents a console logger implementation of the <see cref="ILogger"/> interface.
    /// </summary>
    public class ConsoleLogger : ILogger, IDisposable
    {
        private const int IndentationLength = 2;
        private const char IndentationSymbol = ' ';
        private const int MaxIndentation = 20;

        // Locker is needed for thread-safety.
        private readonly SemaphoreSlim _locker = new SemaphoreSlim(1, 1);

        /// <inheritdoc />
        public int Indentation { get; private set; }

        /// <inheritdoc cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            _locker.Dispose();
        }

        /// <inheritdoc />
        public void IndentDecrease()
        {
            RunUnderWaitEvent(() =>
                {
                    if (Indentation > 0)
                        Indentation--;
                });
        }

        /// <inheritdoc />
        public void IndentIncrease()
        {
            RunUnderWaitEvent(() =>
            {
                if (Indentation < MaxIndentation)
                    Indentation++;
            });
        }

        /// <inheritdoc />
        public void WriteError(string msg)
        {
            RunUnderWaitEvent(() =>
            {
                WriteIndentation();
                WriteMessage(msg, ConsoleColor.DarkRed);
            });
        }

        /// <inheritdoc />
        public void WriteInfo(string msg)
        {
            RunUnderWaitEvent(() =>
            {
                WriteIndentation();
                WriteMessage(msg, ConsoleColor.DarkGray);
            });
        }

        /// <inheritdoc />
        public void WriteWarning(string msg)
        {
            RunUnderWaitEvent(() =>
            {
                WriteIndentation();
                WriteMessage(msg, ConsoleColor.DarkYellow);
            });
        }

        private static void WriteMessage(string msg, ConsoleColor foregroundColor)
        {
            var origColor = Console.ForegroundColor;
            Console.ForegroundColor = foregroundColor;

            Console.WriteLine(msg);

            Console.ForegroundColor = origColor;
        }

        private void RunUnderWaitEvent(Action action)
        {
            _locker.Wait();
            action();
            _locker.Release();
        }

        private void WriteIndentation()
        {
            Console.Write(new string(Enumerable.Repeat(IndentationSymbol, IndentationLength * Indentation).ToArray()));
        }
    }
}