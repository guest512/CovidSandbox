using System;

namespace CovidSandbox.Model.Reports.Intermediate
{
    public class ProvinceIntermediateReport : IntermediateReport
    {
        public string Country { get; }

        protected ProvinceIntermediateReport(string name, string countryName, DateTime day) : base(name,
            day)
        {
            Country = countryName;
        }

        public ProvinceIntermediateReport(string name, string countryName, DateTime day, Metrics total, Metrics change) : base(name, day, total, change)
        {
            Country = countryName;
        }

        public override string ToString() => $"{Country}({Name}), {Day}: Total({Total}) Change({Change})";
    }
}