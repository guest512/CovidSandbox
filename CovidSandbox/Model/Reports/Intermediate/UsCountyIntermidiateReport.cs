using System;

namespace CovidSandbox.Model.Reports.Intermediate
{
    public class UsCountyIntermidiateReport : ProvinceIntermediateReport
    {
        public uint FIPS { get; }

        public string Province { get; }

        public UsCountyIntermidiateReport(uint fips, string name, string provinceName, string countryName, DateTime day, Metrics total, Metrics change) : base(name, countryName, day, total, change)
        {
            FIPS = fips;
            Province = provinceName;
        }

        public override string ToString() => $"{Country}({Province}-{Name}), {Day}: Total({Total}) Change({Change})";
    }
}