using System;

namespace CovidSandbox.Model.Reports.Intermediate
{
    public class ProvinceIntermediateReport : IntermediateReport
    {
        public string Country { get; }

        public ProvinceIntermediateReport(string provinceName, string countryName, DateTime day, Metrics total, Metrics change) : base(provinceName, day, total, change)
        {
            Country = countryName;
        }

        public override string ToString() => $"{Country}({Name}), {Day}: Total({Total}) Change({Change})";
    }
}