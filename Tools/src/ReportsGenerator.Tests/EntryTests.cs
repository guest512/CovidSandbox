using NUnit.Framework;
using ReportsGenerator.Data;
using ReportsGenerator.Model;
using ReportsGenerator.Model.Processors;
using ReportsGenerator.Utils;
using System;
using System.Collections.Generic;

namespace ReportsGenerator.Tests
{
    [TestFixture]
    public class EntryTests
    {
        private EntryFactory _factory = null!;

        [OneTimeSetUp]
        public void Setup()
        {
            _factory = new EntryFactory(new Dictionary<RowVersion, IRowProcessor>
            {
                {RowVersion.JHopkinsV1, new JHopkinsTestRowProcessor()},
                {RowVersion.JHopkinsV2, new JHopkinsTestRowProcessor()},
                {RowVersion.JHopkinsV3, new JHopkinsTestRowProcessor()},
                {RowVersion.JHopkinsV4, new JHopkinsTestRowProcessor()},
                {RowVersion.YandexRussia, new YandexRussiaRowProcessor(new TestStatsProvider(),new  NullLogger())},
            }, new NullLogger());
        }

        [Test]
        public void EntryProcessesCountiresNamesOnCreation()
        {
            var countriesToProcess = new Dictionary<string, string>()
            {
                {" Azerbaijan", "Azerbaijan"},
                {"Russian Federation", "Russia"},
                {"Viet Nam", "Vietnam"},
                {"United Kingdom", "UK"},
                {"Taiwan*", "Taiwan"},
                {"Gambia, The", "Gambia"},
                {"The Gambia", "Gambia"},
                {"Korea, South", "South Korea"},
                {"Macao SAR", "Macau"},
                {"Iran (Islamic Republic of)", "Iran"},
                {"Hong Kong SAR", "Hong Kong"},
                {"Bahamas, The", "Bahamas"},
                {"The Bahamas", "Bahamas"},
                {"Mainland China", "China"},
                {"Taipei and environs", "Taiwan"},
                {"St. Martin", "Saint Martin"},
                {"Republic of the Congo", "Congo (Brazzaville)"},
                {"Republic of Moldova", "Moldova"},
                {"Republic of Ireland", "Ireland"},
            };

            foreach (var (countryFromReport, actualCountryName) in countriesToProcess)
            {
                var row = new Row(new[]
                {
                    new CsvField(Field.CountryRegion, countryFromReport),
                }, RowVersion.JHopkinsV1);

                Assert.That(_factory.CreateEntry(row).CountryRegion, Is.EqualTo(actualCountryName));
            }
        }

        [Test]
        public void ValidateEntryGetHashCodeOverload()
        {
            var entry = _factory.CreateEntry(new Row(new[]
            {
                new CsvField(Field.CountryRegion, "Russia"),
                new CsvField(Field.Deaths, "5"),
                new CsvField(Field.LastUpdate, "2/22/2020 2:20"),
            }, RowVersion.JHopkinsV1));

            var hashCode = new HashCode();
            hashCode.Add(""); //County
            hashCode.Add((long)-5); //Active
            hashCode.Add(0); //Confirmed
            hashCode.Add("Russia"); //CountryRegion
            hashCode.Add(5); //Deaths
            hashCode.Add(new DateTime(2020, 2, 22).Date); //LastUpdate
            hashCode.Add("Main territory"); //ProvinceState
            hashCode.Add(0); //Recovered
            hashCode.Add(Origin.JHopkins); //Origin
            hashCode.Add(2); //IsoLevel
            hashCode.Add("TEST NAME"); //StatsName

            Assert.That(entry.GetHashCode(), Is.EqualTo(hashCode.ToHashCode()));
        }

        [Test]
        public void ValidateEntryOperatorsOverload()
        {
            var e1 = _factory.CreateEntry(new Row(new[]
            {
                new CsvField(Field.CountryRegion, "Russia"),
                new CsvField(Field.Deaths, "5"),
                new CsvField(Field.LastUpdate, "2/22/2020 2:20"),
            }, RowVersion.JHopkinsV1));

            var e2 = _factory.CreateEntry(new Row(new[]
            {
                new CsvField(Field.CountryRegion, "Russia"),
                new CsvField(Field.ProvinceState, "Kamchatka Krai"),
                new CsvField(Field.Deaths, "5"),
                new CsvField(Field.LastUpdate, "2/22/2020 2:20"),
            }, RowVersion.JHopkinsV1));
            var e3 = _factory.CreateEntry(new Row(new[]
            {
                new CsvField(Field.CountryRegion, "Russia"),
                new CsvField(Field.Deaths, "5"),
                new CsvField(Field.LastUpdate, "2/22/2020 2:20"),
            }, RowVersion.JHopkinsV1));

            Assert.That(e2 != e1, Is.True);
            Assert.That(e2 == e1, Is.False);
            Assert.That(e1.Equals(e2), Is.False);

            Assert.That(e1 != e3, Is.False);
            Assert.That(e1 == e3, Is.True);
            Assert.That(e3.Equals(e1), Is.True);
        }

        [Test]
        public void ValidateEntryToStringOverload()
        {
            var entry = _factory.CreateEntry(new Row(new[]
            {
                new CsvField(Field.CountryRegion, "Russia"),
                new CsvField(Field.Deaths, "5"),
                new CsvField(Field.LastUpdate, "2/22/2020 2:20"),
            }, RowVersion.JHopkinsV1));
            Assert.That(entry.ToString(),
                Is.EqualTo($"JHopkins-Russia(Main territory), {new DateTime(2020, 2, 22).ToShortDateString()}: 0--5-0-5"));

            entry = _factory.CreateEntry(new Row(new[]
            {
                new CsvField(Field.CountryRegion, "Russia"),
                new CsvField(Field.ProvinceState, "Kamchatka Krai"),
                new CsvField(Field.Deaths, "5"),
                new CsvField(Field.LastUpdate, "2/22/2020 2:20"),
            }, RowVersion.JHopkinsV1));
            Assert.That(entry.ToString(),
                Is.EqualTo($"JHopkins-Russia(TEST CYRILLIC NAME), {new DateTime(2020, 2, 22).ToShortDateString()}: 0--5-0-5"));
        }
    }
}