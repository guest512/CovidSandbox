using System;

namespace ReportsGenerator.Model.Reports.Intermediate
{
    public record BasicReport
    {
        public DateTime Day { get; init; }

        public string Name { get; init; } = string.Empty;

        public string Parent { get; init; } = string.Empty;

        public Metrics Total { get; init; } = Metrics.Empty;

        public override string ToString() => $"{Name}, {Day}: Total({Total})";
    }
}