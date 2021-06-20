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
        private readonly SemaphoreSlim _locker = new(1, 1);

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
            WriteMessage(msg, ConsoleColor.DarkRed);
        }

        /// <inheritdoc />
        public void WriteInfo(string msg)
        {
            WriteMessage(msg, ConsoleColor.DarkGray);
        }

        /// <inheritdoc />
        public void WriteWarning(string msg)
        {
            WriteMessage(msg, ConsoleColor.DarkYellow);
        }

        private void WriteMessage(string msg, ConsoleColor foregroundColor)
        {
            var messageText =
                $"{new string(Enumerable.Repeat(IndentationSymbol, IndentationLength * Indentation).ToArray())}{msg}";

            RunUnderWaitEvent(() =>
            {
                var origColor = Console.ForegroundColor;
                Console.ForegroundColor = foregroundColor;

                Console.WriteLine(messageText);

                Console.ForegroundColor = origColor;
            });
        }

        private void RunUnderWaitEvent(Action action)
        {
            _locker.Wait();
            action();
            _locker.Release();
        }
    }
}