using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ReportsGenerator.Model;
using ReportsGenerator.Model.Reports;
using ReportsGenerator.Model.Reports.Intermediate;

namespace ReportsGenerator.Tests
{
    [TestFixture]
    public class ReportsTests
    {
        private BasicReportsWalker? walker;
        private StatsReport? structure;

        [OneTimeSetUp]
        public void InitReports()
        {
            static BasicReport CreateReport(DateTime day, in long confirmed) =>
                new()
                {
                    Name = "Test",
                    Day = day,
                    Total = new Metrics(confirmed, 0, 0, 0)
                };

            var data = new long[]
            {
                33846, 163,159,157,162,160,181,183,180,182,179,181,184,187,189,186
            };

            var startDate = new DateTime(2020, 8, 15);

            var reports = new List<BasicReport>(data.Length);

            for (var i = 0; i < data.Length; i++)
            {
                reports.Add(CreateReport(startDate.AddDays(i), 33846 + data[..(i+1)].Sum()));
            }

            structure = new StatsReport("Test", "Test", new TestStatsProvider());
            walker = new BasicReportsWalker(reports, structure);

        }

      

        [Test]
        public void ValidateRt()
        {
            var result = new[]
            {
                0,
                0,
                0,
                0,
                0.009469,
                0.009402,
                0.009703,
                0.010062,
                1.098284,
                1.137931,
                1.096970,
                1.052478,
                1.031250,
                1.006887,
                1.023481,
                1.033241,
            };
            const double eps = 1e-5;
            var countryReport = new CountryReport("test", walker!, structure!);
            var i = 0;


            var actual = new[] {walker!.StartDay, walker.LastDay}.GetContinuousDateRange()
                .Select(day => countryReport.GetRt(day)).ToArray();

            CollectionAssert.AreEqual(result,actual,new DoubleComparer(eps));
        }

        private class DoubleComparer : IComparer
        {
            private readonly double _epsilon;

            public DoubleComparer(double epsilon)
            {
                _epsilon = epsilon;
            }

            public int Compare(object? x, object? y)
            {
                var x_num = (double) x;
                var y_num = (double) y;

                if (Math.Abs(x_num - y_num) < _epsilon)
                    return 0;

                if (x_num > y_num)
                    return 1;

                return -1;
            }
        }
    }
}
