﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace ReportsGenerator.Model.Reports.Intermediate
{
    public class LinkedReport
    {
        private readonly Metrics _total;

        public LinkedReport(string name, DateTime day, IsoLevel level) : this(name, day, level, Metrics.Empty)
        {
        }

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

        public static LinkedReport Empty { get; } = new LinkedReport(string.Empty, DateTime.MinValue, IsoLevel.CountryRegion, Metrics.Empty);

        public ICollection<LinkedReport> Children { get; }
        public DateTime Day { get; }
        public IsoLevel Level { get; }
        public string Name { get; }
        public LinkedReport Next { get; set; }
        public LinkedReport Parent { get; set; }
        public LinkedReport Previous { get; set; }

        public Metrics Total
        {
            get
            {
                return _total == Metrics.Empty ? Children.Aggregate(Metrics.Empty, (sum, child) => sum + child.Total) : _total;
            }
        }

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