using Moq;
using NUnit.Framework;
using ReportsGenerator.Data;
using ReportsGenerator.Model;
using ReportsGenerator.Model.Processors;
using ReportsGenerator.Utils;

namespace ReportsGenerator.Tests
{
	[TestFixture]
	public class RowProcessorTests
	{
		private Mock<BaseRowProcessor> _rowProcessor = null!;

		[OneTimeSetUp]
		public void Setup()
		{
			_rowProcessor = new Mock<BaseRowProcessor>(new Mock<IStatsProvider>().Object, new NullLogger());
			_rowProcessor.Setup(x => x.GetCountryName(It.Is<Row>(r => r[FieldId.CountryRegion] == "Country"))).Returns("TEST COUNTRY");
			_rowProcessor.Setup(x => x.GetProvinceName(It.Is<Row>(r => r[FieldId.ProvinceState] == "Province"))).Returns("TEST PROVINCE");
			_rowProcessor.Setup(x => x.GetCountyName(It.Is<Row>(r => r[FieldId.Admin2] == "County"))).Returns("TEST COUNTY");
		}

		[Test]

		[TestCase("", "Province", "County", default)]
		[TestCase("", "Province", "", default)]
		[TestCase("", "", "County", default)]
		[TestCase("Country", "", "County", IsoLevel.CountryRegion)]
		[TestCase("Country", "Province", "", IsoLevel.ProvinceState)]
		[TestCase("Country", "Province", "County", IsoLevel.County)]
		public void ValidateIsoLevel(string country, string province, string county, IsoLevel expectedLevel)
		{
			var row = new Row(new[]
			{
				new Field(FieldId.CountryRegion, country),
				new Field(FieldId.ProvinceState, province),
				new Field(FieldId.Admin2, county)
			}, RowVersion.Unknown);

			if (string.IsNullOrWhiteSpace(country))
			{
				Assert.That(()=>_rowProcessor.Object.GetIsoLevel(row),Throws.InvalidOperationException);
			}
			else
			{
				Assert.That(_rowProcessor.Object.GetIsoLevel(row), Is.EqualTo(expectedLevel));
			}
		}
	}
}