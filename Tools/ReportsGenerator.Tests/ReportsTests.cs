using System;
using System.Collections.Generic;
using System.Text;
using CovidSandbox.Model;
using CovidSandbox.Model.Reports;
using CovidSandbox.Model.Reports.Intermediate;
using NUnit.Framework;

namespace CovidSandbox.Tests
{
    [TestFixture]
    public class ReportsTests
    {
        private LinkedReport? _head;

        [OneTimeSetUp]
        public void InitReports()
        {
            static LinkedReport CreateReport(DateTime day, in long confirmed) =>
                new LinkedReport("Test", day, IsoLevel.CountryRegion,
                    new Metrics(confirmed, 0, 0, 0));

            var data = new long[]
            {
                34009,163,159,157,162,160,181,183,180,182,179,181,184,187,189,186
            };

            var startDate = new DateTime(2020, 8, 15);

            _head = CreateReport(startDate, 34009+
                                            data[0]);

            var position = _head;
            var day = startDate;

            for (var i = 1; i < data.Length; i++)
            {
                position.Next = CreateReport(day.AddDays(1), position.Total.Confirmed + data[i]);
                position.Next.Previous = position;

                position = position.Next;
                day = position.Day;
            }

            _head = _head.Next;

        }

      

        [Test]
        public void ValidateRt()
        {
            var position = _head!;
            var result = new[]
            {
                0,
                0,
                0,
                0,
                0,
                0,
                0,
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
            var countryReport = new CountryReport("test", _head!);
            var i = 0;
            while (position.Next != LinkedReport.Empty)
            {
                Assert.AreEqual(result[i],countryReport.GetRt(position.Day), eps);

                i++;
                position = position.Next;
            }
        }
    }
}
