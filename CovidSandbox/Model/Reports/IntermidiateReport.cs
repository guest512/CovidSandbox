using System;

namespace CovidSandbox.Model.Reports
{
    public class IntermidiateReport
    {
        public string Name { get; }

        public DateTime Day { get; }

        public virtual Metrics Total { get; }

        public virtual Metrics Change { get; }

        protected IntermidiateReport(string countryProvinceName, DateTime day)
        {
            Name = countryProvinceName;
            Day = day.Date;
        }

        public IntermidiateReport(string coutnryProvinceName, DateTime day, Metrics total, Metrics change) : this(coutnryProvinceName, day)
        {
            Total = total;
            Change = change;
        }
    }
}