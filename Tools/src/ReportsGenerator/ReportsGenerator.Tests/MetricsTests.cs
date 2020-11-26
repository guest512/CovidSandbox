using System.Collections.Generic;
using NUnit.Framework;
using ReportsGenerator.Data;
using ReportsGenerator.Model;
using ReportsGenerator.Model.Processors;
using ReportsGenerator.Utils;

namespace ReportsGenerator.Tests
{
    [TestFixture]
    public class MetricsTests
    {
        [Test]
        public void ValidateEmptyMetrics()
        {
            Assert.That(Metrics.Empty, Is.EqualTo(new Metrics(0, 0, 0, 0)));
        }

        [Test]
        public void ValidateMetricsDeconstruc()
        {
            var metrics = new Metrics(1, 2, 3, 4);

            var (c, a, r, d) = metrics;

            Assert.That(c, Is.EqualTo(metrics.Confirmed));
            Assert.That(a, Is.EqualTo(metrics.Active));
            Assert.That(r, Is.EqualTo(metrics.Recovered));
            Assert.That(d, Is.EqualTo(metrics.Deaths));
        }

        [Test]
        public void ValidateMetricsOperatorsOverload()
        {
            var m1 = new Metrics(1, 2, 3, 4);
            var m2 = new Metrics(5, 6, 7, 8);
            var m3 = new Metrics(1, 2, 3, 4);

            Assert.That(m1 + m2, Is.EqualTo(new Metrics(6, 8, 10, 12)));
            Assert.That(m2 - m1, Is.EqualTo(new Metrics(4, 4, 4, 4)));

            Assert.That(m2 != m1, Is.True);
            Assert.That(m2 == m1, Is.False);
            Assert.That(m1.Equals(m2), Is.False);

            Assert.That(m1 != m3, Is.False);
            Assert.That(m1 == m3, Is.True);
            Assert.That(m3.Equals(m1), Is.True);
        }

        [Test]
        public void ValidateMetricsToStringOverload()
        {
            var metrics = new Metrics(1, 2, 3, 4);

            Assert.That(metrics.ToString(), Is.EqualTo("1 2 3 4"));
        }
    }
}