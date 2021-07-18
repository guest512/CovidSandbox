using System;
using System.Collections.Generic;
using System.Linq;

namespace ReportsGenerator.Data
{
    /// <summary>
    /// Represents an abstraction layer for the set of <see cref="IFormattableReport{TRow, TName}"/>.
    /// </summary>
    /// <typeparam name="TName">Report names type parameter.</typeparam>
    public class MergedFormattableReport<TName> : IFormattableReport<int, TName>
    {
        private readonly List<IFormattableReport<int, TName>> _reports = new();
        private readonly Dictionary<int, Func<string, int, object>> _rowIdReportMap = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="MergedFormattableReport{TName}"/>.
        /// </summary>
        /// <param name="reports">Reports to merge.</param>
        public MergedFormattableReport(IEnumerable<IFormattableReport<int, TName>> reports)
        {
            _reports.AddRange(reports);
        }

        /// <inheritdoc/>
        public IEnumerable<TName> Name => _reports[0].Name;

        /// <inheritdoc/>
        public IEnumerable<string> Properties => _reports[0].Properties;

        /// <inheritdoc/>
        public IEnumerable<int> RowIds => GetRowIds();

        /// <inheritdoc/>
        public ReportType ReportType => _reports[0].ReportType;

        /// <inheritdoc/>
        public object GetValue(string property, int key) => GetValueImpl(property, key);

        private IEnumerable<int> GetRowIds()
        {
            var offset = 0;

            foreach (var report in _reports)
            {
                foreach (var rowId in report.RowIds)
                {
                    yield return rowId + offset;
                }

                offset += report.RowIds.Count();
            }
        }

        private object GetValueImpl(string property, int key)
        {
            if (_rowIdReportMap.ContainsKey(key))
            {
                return _rowIdReportMap[key](property, key);
            }

            var offset = 0;
            foreach (var report in _reports)
            {
                var idMax = report.RowIds.Max();
                if (key > idMax)
                {
                    var count = report.RowIds.Count();
                    offset += count;
                    key -= count;
                    continue;
                }

                foreach (var id in report.RowIds)
                {
                    _rowIdReportMap.Add(offset + id, (p, k) => { return report.GetValue(p, k - offset); });
                }
                break;
            }

            return _rowIdReportMap[key](property, key);
        }
    }
}