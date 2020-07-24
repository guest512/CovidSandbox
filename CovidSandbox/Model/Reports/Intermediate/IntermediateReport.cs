using System;

namespace CovidSandbox.Model.Reports.Intermediate
{
    public class IntermediateReport
    {
        public string Name { get; }

        public DateTime Day { get; }

        public Metrics Total { get; protected set; }

        public Metrics Change { get; protected set; }

        protected IntermediateReport(string countryProvinceName, DateTime day)
        {
            Name = countryProvinceName;
            Day = day.Date;
        }

        public IntermediateReport(string coutnryProvinceName, DateTime day, Metrics total, Metrics change) : this(coutnryProvinceName, day)
        {
            Total = total;
            Change = change;
        }

        public override string ToString() => $"{Name}, {Day}: Total({Total}) Change({Change})";
    }
}