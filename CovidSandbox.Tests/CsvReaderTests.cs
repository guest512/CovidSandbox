using CovidSandbox.Data;
using NUnit.Framework;
using System.IO;
using System.Linq;

namespace CovidSandbox.Tests
{
    [TestFixture]
    public class CsvReaderTests
    {
        [Test]
        public void ValidateEmptyDataStream()
        {
            using var ms = new MemoryStream();
            using var sr = new StreamReader(ms);

            var csvReader = new CsvReader();
            Assert.That(csvReader.Read(sr).Count(), Is.EqualTo(0));
        }

        [Test]
        public void ValidateJHopkinsV1File()
        {
            using var ms = new MemoryStream();

            using (var sw = new StreamWriter(ms, leaveOpen: true))
            {
                sw.WriteLine("Province/State,Country/Region,Last Update,Confirmed,Deaths,Recovered");
                sw.WriteLine("Fujian,Mainland China,1/25/20 17:00,18,,");
                sw.WriteLine("Tianjin,Mainland China,1/25/20 17:00,10,,");
            }

            ms.Position = 0;

            using var sr = new StreamReader(ms);

            var csvReader = new CsvReader();

            var rows = csvReader.Read(sr).ToList();

            Assert.That(rows.Count, Is.EqualTo(2));
            Assert.That(rows[1][Field.CountryRegion], Is.EqualTo("Mainland China"));
            Assert.That(rows[0][Field.ProvinceState], Is.EqualTo("Fujian"));
            Assert.That(rows[0][Field.Recovered], Is.EqualTo(string.Empty));
            Assert.That(rows[1][Field.CaseFatalityRatio], Is.EqualTo(string.Empty));
        }

        [Test]
        public void ValidateJHopkinsV2File()
        {
            using var ms = new MemoryStream();

            using (var sw = new StreamWriter(ms, leaveOpen: true))
            {
                sw.WriteLine("Province/State,Country/Region,Last Update,Confirmed,Deaths,Recovered,Latitude,Longitude");
                sw.WriteLine("Jiangxi,China,2020-03-11T02:18:14,935,1,932,27.6140,115.7221");
                sw.WriteLine(",Sweden,2020-03-11T19:33:02,500,1,1,63.0000,16.0000");
            }

            ms.Position = 0;

            using var sr = new StreamReader(ms);

            var csvReader = new CsvReader();

            var rows = csvReader.Read(sr).ToList();

            Assert.That(rows.Count, Is.EqualTo(2));
            Assert.That(rows[0][Field.CountryRegion], Is.EqualTo("China"));
            Assert.That(rows[1][Field.ProvinceState], Is.EqualTo(string.Empty));
            Assert.That(rows[0][Field.Recovered], Is.EqualTo("932"));
            Assert.That(rows[1][Field.CaseFatalityRatio], Is.EqualTo(string.Empty));
        }

        [Test]
        public void ValidateJHopkinsV3File()
        {
            using var ms = new MemoryStream();

            using (var sw = new StreamWriter(ms, leaveOpen: true))
            {
                sw.WriteLine("FIPS,Admin2,Province_State,Country_Region,Last_Update,Lat,Long_,Confirmed,Deaths,Recovered,Active,Combined_Key");
                sw.WriteLine("28001,Adams,Mississippi,US,2020-05-02 02:32:27,31.47669768,-91.35326037,148,8,0,140,\"Adams, Mississippi, US\"");
                sw.WriteLine(",Michigan Department of Corrections (MDOC),Michigan,US,2020-05-02 02:32:27,,,1560,42,0,1518,\"Michigan Department of Corrections (MDOC), Michigan, US\"");
            }

            ms.Position = 0;

            using var sr = new StreamReader(ms);

            var csvReader = new CsvReader();

            var rows = csvReader.Read(sr).ToList();

            Assert.That(rows.Count, Is.EqualTo(2));
            Assert.That(rows[0][Field.CountryRegion], Is.EqualTo("US"));
            Assert.That(rows[1][Field.ProvinceState], Is.EqualTo("Michigan"));
            Assert.That(rows[0][Field.Recovered], Is.EqualTo("0"));
            Assert.That(rows[1][Field.CaseFatalityRatio], Is.EqualTo(string.Empty));
            Assert.That(rows[0][Field.FIPS], Is.EqualTo("28001"));
        }

        [Test]
        public void ValidateJHopkinsV4File()
        {
            using var ms = new MemoryStream();

            using (var sw = new StreamWriter(ms, leaveOpen: true))
            {
                sw.WriteLine("FIPS,Admin2,Province_State,Country_Region,Last_Update,Lat,Long_,Confirmed,Deaths,Recovered,Active,Combined_Key,Incidence_Rate,Case-Fatality_Ratio");
                sw.WriteLine("16003,Adams,Idaho,US,2020-06-04 02:33:14,44.89333571,-116.4545247,3,0,0,3,\"Adams, Idaho, US\",69.86492780624127,0.0");
                sw.WriteLine(",,Mykolaiv Oblast,Ukraine,2020-06-04 02:33:14,46.975,31.9946,295,7,182,106,\"Mykolaiv Oblast, Ukraine\",11.696968423339854,2.3728813559322033");
            }

            ms.Position = 0;

            using var sr = new StreamReader(ms);

            var csvReader = new CsvReader();

            var rows = csvReader.Read(sr).ToList();

            Assert.That(rows.Count, Is.EqualTo(2));
            Assert.That(rows[0][Field.CountryRegion], Is.EqualTo("US"));
            Assert.That(rows[1][Field.ProvinceState], Is.EqualTo("Mykolaiv Oblast"));
            Assert.That(rows[0][Field.Recovered], Is.EqualTo("0"));
            Assert.That(rows[1][Field.CaseFatalityRatio], Is.EqualTo("2.3728813559322033"));
            Assert.That(rows[0][Field.FIPS], Is.EqualTo("16003"));
        }

        [Test]
        public void ValidateUnknownFile()
        {
            using var ms = new MemoryStream();

            using (var sw = new StreamWriter(ms, leaveOpen: true))
            {
                sw.WriteLine("Province-State,Country/Region,Last Update,Confirmed,Deaths,Recovered,Latitude,Longitude");
                sw.WriteLine("Jiangxi,China,2020-03-11T02:18:14,935,1,932,27.6140,115.7221");
                sw.WriteLine(",Sweden,2020-03-11T19:33:02,500,1,1,63.0000,16.0000");
            }

            ms.Position = 0;

            using var sr = new StreamReader(ms);

            var csvReader = new CsvReader();

            Assert.That(() => csvReader.Read(sr).ToList(), Throws.Exception);
        }
    }
}