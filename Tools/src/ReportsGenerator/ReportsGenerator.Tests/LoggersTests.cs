using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using ReportsGenerator.Utils;

namespace ReportsGenerator.Tests
{
	[TestFixture]
	public class LoggersTests
	{
		[Test]
		public void NullLoggerWorks()
		{
			var logger = new NullLogger();
			Assert.That(logger.Indentation, Is.Zero);
		}

		[Test, SuppressMessage("ReSharper", "AccessToDisposedClosure")]
		public async Task ConsoleLoggerNotFail()
		{
			using var logger = new ConsoleLogger();
			var state = Parallel.ForEach(Enumerable.Range(1, 5), i =>
			{
				logger.IndentIncrease();
				logger.WriteInfo($"Test {i}");
				logger.WriteWarning($"Warn {i}");
				logger.WriteError($"Error {i}");
				logger.IndentDecrease();
			});

			while (!state.IsCompleted)
			{
				await Task.Delay(500);
			}
		}
	}
}
