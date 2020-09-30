using System;
using System.Collections.Generic;
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
            Assert.That(provider.GetVersion(CsvReader.SplitRowString(header)), Is.EqualTo(version));
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
            Assert.That(Convertors.ParseDate(data.Key), Is.EqualTo(data.Value));
        }

        [Test]
        public void ValidateDateParserUnsupportedDate()
        {
            Assert.That(() => Convertors.ParseDate(DateTime.Now.ToShortTimeString()), Throws.Exception);
        }

        [Test]
        [TestCase("Test", "Test")]
        //[TestCase(null, "")] // Seems like an invalid test case
        [TestCase("", "")]
        [TestCase("Test, Test", "\"Test, Test\"")]
        public void ValidateToCsvString(string input, string expected)
        {
            Assert.That(input.ToCsvString(), Is.EqualTo(expected));
        }
    }
}