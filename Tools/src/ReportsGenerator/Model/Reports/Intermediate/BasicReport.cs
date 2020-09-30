using System;

namespace ReportsGenerator.Model.Reports.Intermediate
{
    public readonly struct BasicReport
    {
        public BasicReport(string name, string parent, DateTime day, Metrics total)
        {
            Name = name;
            Parent = parent;
            Day = day;
            Total = total;
        }

        public BasicReport(string name, DateTime day, Metrics total) : this(name, string.Empty, day, total)
        {
        }

        public DateTime Day { get; }
        public string Name { get; }
        public string Parent { get; }
        public Metrics Total { get; }

        public override string ToString() => $"{Name}, {Day}: Total({Total})";
    }
}