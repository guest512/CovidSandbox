using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ReportsGenerator.Data;
using ReportsGenerator.Data.Providers;
using ReportsGenerator.Utils;

namespace ReportsGenerator.Tests
{
    public class DataTests
    {
        [Test]
        [TestCase("Province,Last Update,Confirmed,Deaths,Recovered", RowVersion.Unknown)]
        [TestCase("Province,Country/Region,Last Update,Confirmed,Deaths,Recovered", RowVersion.Unknown)]
        [TestCase("Province/State,Country/Region,Last Update,Confirmed,Deaths,Recovered", RowVersion.JHopkinsV1)]
        [TestCase("Province/State,Country/Region,Last Update,Confirmed,Deaths,Recovered,Latitude,Longitude", RowVersion.JHopkinsV2)]
        [TestCase("FIPS,Admin2,Province_State,Country_Region,Last_Update,Lat,Long_,Confirmed,Deaths,Recovered,Active,Combined_Key", RowVersion.JHopkinsV3)]
        [TestCase("FIPS,Admin2,Province_State,Country_Region,Last_Update,Lat,Long_,Confirmed,Deaths,Recovered,Active,Combined_Key,Incidence_Rate,Case-Fatality_Ratio", RowVersion.JHopkinsV4)]
        public void ValidateJHopkinsVersionStrings(string header, RowVersion version)
        {
            var provider = new JHopkinsDataProvider();
            Assert.That(provider.GetVersion(header.SplitCsvRowString().ToArray()), Is.EqualTo(version));
        }

        [Test]
        [TestCaseSource(nameof(SpliRowStringSource))]
        public void ValidateSplitRowStrings(KeyValuePair<string, string[]> row)
        {
            var splittedRow = row.Key.SplitCsvRowString().ToArray();

            CollectionAssert.AreEqual(row.Value,splittedRow);
        }

        public static IEnumerable<KeyValuePair<string, string[]>> SpliRowStringSource()
        {
            yield return new KeyValuePair<string, string[]>
            (
                "Province,Last Update,Confirmed,Deaths,Recovered",
                new[] {"Province", "Last Update", "Confirmed", "Deaths", "Recovered"}
            );

            yield return new KeyValuePair<string, string[]>
            (
                "FIPS,Admin2,Province_State,Country_Region,Last_Update,Lat,Long_,Confirmed,Deaths,Recovered,Active,Combined_Key,Incidence_Rate,Case-Fatality_Ratio",
                new[]
                {
                    "FIPS", "Admin2", "Province_State", "Country_Region", "Last_Update", "Lat", "Long_", "Confirmed",
                    "Deaths", "Recovered", "Active", "Combined_Key", "Incidence_Rate", "Case-Fatality_Ratio"
                }
            );

            yield return new KeyValuePair<string, string[]>
            (
                "09.03.2020,Калининградская обл.,1,0,0,0,0,0",
                new[]
                {
                    "09.03.2020", "Калининградская обл.", "1", "0", "0", "0", "0", "0"
                }
            );

            yield return new KeyValuePair<string, string[]>
            (
                "Anhui,Mainland China,1/23/20 17:00,9,,",
                new[]
                {
                    "Anhui", "Mainland China", "1/23/20 17:00", "9", string.Empty, string.Empty
                }
            );

            yield return new KeyValuePair<string, string[]>
            (
                ",Japan,1/23/20 17:00,1,,",
                new[]
                {
                    string.Empty, "Japan", "1/23/20 17:00", "1", string.Empty, string.Empty
                }
            );

            yield return new KeyValuePair<string, string[]>
            (
                "\"Washington, D.C.\",US,2020-03-08T13:53:03,2,0,0,38.9072,-77.0369",
                new[]
                {
                    "Washington, D.C.", "US", "2020-03-08T13:53:03", "2", "0", "0", "38.9072", "-77.0369",
                }
            );

            yield return new KeyValuePair<string, string[]>
            (
                "\"Travis, CA (From Diamond Princess)\",US,2020-02-24T23:33:02,0,0,0,38.2721,-121.9399",
                new[]
                {
                    "Travis, CA (From Diamond Princess)", "US", "2020-02-24T23:33:02", "0", "0", "0", "38.2721", "-121.9399",
                }
            );
        }

        [Test]
        public void JHopkinsDataProviderDoesntFailOnGetFields([Values] RowVersion version)
        {
            var provider = new JHopkinsDataProvider();
            Assert.DoesNotThrow(() => { provider.GetFields(version); });
        }

        public static KeyValuePair<string, DateTime>[] Dates = {
            new KeyValuePair<string, DateTime>("1/31/2020 23:59",new DateTime(2020,1,31)),
            new KeyValuePair<string, DateTime>("2/1/2020 13:33", new DateTime(2020,2,1)),
            new KeyValuePair<string, DateTime>("2/1/2020 1:52", new DateTime(2020,2,1)),

            new KeyValuePair<string, DateTime>("3/22/20 23:45", new DateTime(2020,3,22)),
            new KeyValuePair<string, DateTime>("4/4/20 23:34", new DateTime(2020,4,4)),

            new KeyValuePair<string, DateTime>("2020-06-05 02:33:06", new DateTime(2020,6,5)),
            new KeyValuePair<string, DateTime>("2020-03-21T20:43:02", new DateTime(2020,3,21)),

            new KeyValuePair<string, DateTime>("03-21-2020", new DateTime(2020,3,21)),
            new KeyValuePair<string, DateTime>("22.04.2020", new DateTime(2020,4,22)), 
        };

        [Test]
        [TestCaseSource(nameof(Dates))]
        public void ValidateDateParser(KeyValuePair<string, DateTime> data)
        {
            Assert.That(data.Key.AsDate(), Is.EqualTo(data.Value));
        }

        [Test]
        public void ValidateDateParserUnsupportedDate()
        {
            Assert.That(() => DateTime.Now.ToShortTimeString().AsDate(), Throws.Exception);
        }
    }
}