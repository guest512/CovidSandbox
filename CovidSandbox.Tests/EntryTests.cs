using CovidSandbox.Data;
using CovidSandbox.Model;
using CovidSandbox.Model.Processors;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace CovidSandbox.Tests
{
    [TestFixture]
    public class EntryTests
    {
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

                Assert.That(new Entry(row, new JHopkinsTestRowProcessor()).CountryRegion, Is.EqualTo(actualCountryName));
            }
        }

        [Test]
        public void ValidateEntryGetHashCodeOverload()
        {
            var entry = new Entry(new Row(new[]
            {
                new CsvField(Field.CountryRegion, "Russia"),
                new CsvField(Field.Deaths, "5"),
                new CsvField(Field.LastUpdate, "2/22/2020 2:20"),
            }, RowVersion.JHopkinsV1), new JHopkinsTestRowProcessor());

            var hashCode = new HashCode();
            hashCode.Add(""); //County
            hashCode.Add(0); //FIPS
            hashCode.Add((long)-5); //Active
            hashCode.Add(0); //Confirmed
            hashCode.Add("Russia"); //CountryRegion
            hashCode.Add(5); //Deaths
            hashCode.Add(new DateTime(2020, 2, 22).Date); //LastUpdate
            hashCode.Add("Main territory"); //ProvinceState
            hashCode.Add(0); //Recovered
            hashCode.Add(Origin.JHopkins); //Origin
            hashCode.Add(2); //IsoLevel
            
            Assert.That(entry.GetHashCode(), Is.EqualTo(hashCode.ToHashCode()));
        }

        [Test]
        public void ValidateEntryOperatorsOverload()
        {
            var e1 = new Entry(new Row(new[]
            {
                new CsvField(Field.CountryRegion, "Russia"),
                new CsvField(Field.Deaths, "5"),
                new CsvField(Field.LastUpdate, "2/22/2020 2:20"),
            }, RowVersion.JHopkinsV1), new JHopkinsTestRowProcessor());

            var e2 = new Entry(new Row(new[]
            {
                new CsvField(Field.CountryRegion, "Russia"),
                new CsvField(Field.ProvinceState, "Kamchatka Krai"),
                new CsvField(Field.Deaths, "5"),
                new CsvField(Field.LastUpdate, "2/22/2020 2:20"),
            }, RowVersion.JHopkinsV1), new JHopkinsTestRowProcessor());

            var e3 = new Entry(new Row(new[]
            {
                new CsvField(Field.CountryRegion, "Russia"),
                new CsvField(Field.Deaths, "5"),
                new CsvField(Field.LastUpdate, "2/22/2020 2:20"),
            }, RowVersion.JHopkinsV1), new JHopkinsTestRowProcessor());

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
            var entry = new Entry(new Row(new[]
            {
                new CsvField(Field.CountryRegion, "Russia"),
                new CsvField(Field.Deaths, "5"),
                new CsvField(Field.LastUpdate, "2/22/2020 2:20"),
            }, RowVersion.JHopkinsV1), new JHopkinsTestRowProcessor());
            Assert.That(entry.ToString(),
                Is.EqualTo($"JHopkins-Russia(Main territory), {new DateTime(2020, 2, 22).ToShortDateString()}: 0--5-0-5"));

            entry = new Entry(new Row(new[]
            {
                new CsvField(Field.CountryRegion, "Russia"),
                new CsvField(Field.ProvinceState, "Kamchatka Krai"),
                new CsvField(Field.Deaths, "5"),
                new CsvField(Field.LastUpdate, "2/22/2020 2:20"),
            }, RowVersion.JHopkinsV1), new JHopkinsTestRowProcessor());
            Assert.That(entry.ToString(),
                Is.EqualTo($"JHopkins-Russia(Камчатский край), {new DateTime(2020, 2, 22).ToShortDateString()}: 0--5-0-5"));
        }
    }
}