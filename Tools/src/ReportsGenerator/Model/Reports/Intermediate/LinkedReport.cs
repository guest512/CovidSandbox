using System;
using System.Collections.Generic;
using System.Linq;

namespace ReportsGenerator.Model.Reports.Intermediate
{
    /// <summary>
    /// Represents an abstraction for intermediate reports graph node.
    /// </summary>
    public class LinkedReport
    {
        private Metrics _total;


        /// <summary>
        /// Initializes a new instance of the <see cref="LinkedReport"/> class.
        /// </summary>
        /// <param name="name">Id. Name of geographical object.</param>
        /// <param name="day">Day for which report contains data.</param>
        /// <param name="level"><see cref="IsoLevel"/> of the report.</param>
        public LinkedReport(string name, DateTime day, IsoLevel level) : this(name, day, level, Metrics.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LinkedReport"/> class.
        /// </summary>
        /// <param name="name">Id. Name of geographical object.</param>
        /// <param name="day">Day for which report contains data.</param>
        /// <param name="level"><see cref="IsoLevel"/> of the report.</param>
        /// <param name="total">Report's data.</param>
        public LinkedReport(string name, DateTime day, IsoLevel level, Metrics total)
        {
            Name = name;
            Day = day;
            _total = total;
            Level = level;

            Next = Empty;
            Previous = Empty;

            Parent = Empty;
            Children = new List<LinkedReport>();
        }

        /// <summary>
        /// Gets an empty <see cref="LinkedReport"/>. Useful for comparison.
        /// </summary>
        public static LinkedReport Empty { get; } = new LinkedReport(string.Empty, DateTime.MinValue, IsoLevel.CountryRegion, Metrics.Empty);

        /// <summary>
        /// Gets a collection of reports with bigger <see cref="Level"/> value.
        /// </summary>
        public ICollection<LinkedReport> Children { get; }

        /// <summary>
        /// Gets a day for which report contains data.
        /// </summary>
        public DateTime Day { get; }

        /// <summary>
        /// Gets an <see cref="IsoLevel"/> of the report.
        /// </summary>
        public IsoLevel Level { get; }

        /// <summary>
        /// Gets an id. Name of geographical object.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets an report with bigger <see cref="Day"/> value.
        /// </summary>
        public LinkedReport Next { get; set; }

        /// <summary>
        /// Gets a report with smaller <see cref="Level"/> value.
        /// </summary>
        public LinkedReport Parent { get; set; }

        /// <summary>
        /// Gets an report with smaller <see cref="Day"/> value.
        /// </summary>
        public LinkedReport Previous { get; set; }

        /// <summary>
        /// Gets the report's data. Can be the data with which it was initialized, or the sum of the <see cref="Children"/> Total property values.
        /// </summary>
        public Metrics Total
        {
            get
            {
                if (_total == Metrics.Empty)
                    _total = Children.Aggregate(Metrics.Empty, (sum, child) => sum + child.Total);

                return _total;
            }
        }

        /// <summary>
        /// Creates a deep copy of the <see cref="LinkedReport"/>. With the specified day and parent.
        /// </summary>
        /// <param name="day"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public LinkedReport Copy(DateTime day, LinkedReport parent)
        {
            var copyReport = new LinkedReport(Name, day, Level, _total)
            {
                Parent = parent,
            };

            foreach (var child in Children)
            {
                copyReport.Children.Add(child.Copy(day, copyReport));
            }

            return copyReport;
        }

        /// <summary>
        /// Returns a collection of all days available through properties <see cref="Next"/> and <see cref="Previous"/> for this instance.
        /// </summary>
        /// <returns>A <see cref="IEnumerable{T}"/> of <see cref="DateTime"/>.</returns>
        public IEnumerable<DateTime> GetAvailableDates()
        {
            LinkedReport position = this;

            while (position.Previous != Empty)
                position = position.Previous;

            while (position != Empty)
            {
                yield return position.Day;
                position = position.Next;
            }
        }
    }
}