using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ReportsGenerator.Utils;

namespace ReportsGenerator.Model.Reports.Intermediate
{
    /// <summary>
    /// Builds a graph from the bunch of <see cref="LinkedReport"/> for the particular country
    /// (reports with <see cref="LinkedReport.Level"/> == <see cref="IsoLevel.CountryRegion"/>),
    /// using the infromation stored in the <see cref="StatsReport"/> instance for the country.
    /// </summary>
    public class ReportsGraphBuilder
    {
        private readonly string _countryName;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportsGraphBuilder"/> class.
        /// </summary>
        /// <param name="countryName">Country for which report will be built.</param>
        /// <param name="logger">A <see cref="ILogger"/> instance.</param>
        public ReportsGraphBuilder(string countryName, ILogger logger)
        {
            _countryName = countryName;
            _logger = logger;
            Reports = new List<BasicReport>();
        }

        /// <summary>
        /// Gets a reports collection from which graph should be build.
        /// </summary>
        public ICollection<BasicReport> Reports { get; }

        /// <summary>
        /// Builds a report graph from the <see cref="Reports"/>.
        /// </summary>
        /// <remarks>
        /// This method build connections
        /// (fills properties <see cref="LinkedReport.Children"/>, <see cref="LinkedReport.Parent"/>,
        /// <see cref="LinkedReport.Next"/>, <see cref="LinkedReport.Previous"/>) for all reports in
        /// <see cref="Reports"/> properties. It fills all gap days in the graph with <see cref="LinkedReport.Empty"/>
        /// or copies from the days when data was represented, and create necessary parent and children <see cref="LinkedReport"/>
        /// objects to make this graph structure the same as "structure" parameter.
        /// </remarks>
        /// <param name="structure">A geographical objects structure and relations for the country.</param>
        /// <returns>The first day available for the country.</returns>
        public LinkedReport Build(StatsReport structure)
        {
            // Converts BasicReport to LinkedReport and preserve Parent id for each report.
            var allReports =
                Reports.Select(br => new LinkedReportWithParent(br.Parent, CreateLinkedReport(br))).ToList();

            Debug.Assert(AssertReports(allReports));

            //
            // The general assumption here is that when we have county reports for the particular day,
            // it means that we don't have a region/province or country report.
            // So the algorithm to build the graph is following:
            // 
            // 1. Starting from the county report level, build parent reports for each day
            //   until you reach the country report level
            //
            // 2.Link all country level reports between each other day-by-day
            //   if any day is missed, then fill the gap with LinkedReport.Empty, or
            //   with copies from previous days
            //
            // 3. Populate missed province reports for each country report for each day
            //
            // 4. Repeat steps 2 and 3 for province and county report levels.
            //

            // Walk through every county report and create province reports, link
            // county report with new parent. 
            PopulateMissedParentReports(allReports, IsoLevel.County, _countryName);
            Debug.Assert(AssertReports(allReports));

            // Walk through every province report and create country reports, link
            // province report with new parent.
            PopulateMissedParentReports(allReports, IsoLevel.ProvinceState, string.Empty);
            Debug.Assert(AssertReports(allReports));

            // Walk through every country report and connect it with each other.
            // Fill gaps with empty reports or with copies from available dates.
            FillNextPrevReports(allReports, IsoLevel.CountryRegion);
            Debug.Assert(AssertReports(allReports));

            // Walk through every country report and validate that it has full
            // children reports collection (province level), using a "structure" as the reference.
            PopulateMissedChildReports(allReports, IsoLevel.CountryRegion, name => structure.GetProvinces());
            Debug.Assert(AssertReports(allReports));

            // Walk through every province report and connect it with each other.
            // Fill gaps with empty reports or with copies from available dates. <-- there should not be any new object created on this step
            FillNextPrevReports(allReports, IsoLevel.ProvinceState);
            Debug.Assert(AssertReports(allReports));

            // Walk through every province report and validate that it has full
            // children reports collection (county level), using a "structure" as the reference.
            PopulateMissedChildReports(allReports, IsoLevel.ProvinceState, structure.GetCounties);
            Debug.Assert(AssertReports(allReports));

            // Walk through every county report and connect it with each other.
            // Fill gaps with empty reports or with copies from available dates. <-- there should not be any new object created on this step
            FillNextPrevReports(allReports, IsoLevel.County);
            Debug.Assert(AssertReports(allReports));

            // Find the most earlier country report and return it.
            return allReports.Where(rep => rep.Report.Level == IsoLevel.CountryRegion).OrderBy(rep => rep.Report.Day).First().Report;
        }

        private static IEnumerable<LinkedReport> CreateParentReports(IEnumerable<LinkedReportWithParent> childReports, string parentName, IsoLevel level)
        {
            return childReports.GroupBy(cr => cr.Report.Day).Select(cr =>
            {
                var parentReport = new LinkedReport(parentName, cr.Key, level);
                foreach (var (_, report) in cr)
                {
                    parentReport.Children.Add(report);
                    report.Parent = parentReport;
                }

                return parentReport;
            });
        }

        private static void FillNextPrevReports(ICollection<LinkedReportWithParent> reports, IsoLevel level)
        {
            foreach (var reportsGroup in reports
                .Where(lr => lr.Report.Level == level)
                .GroupBy(lr => lr.Parent).ToArray())
            {
                var reportsGroupsGrouppedByName = reportsGroup.Select(rg => rg.Report).GroupBy(pcr => pcr.Name).ToArray();
                foreach (var reportsGrouppedByName in reportsGroupsGrouppedByName)
                {
                    var orderedReportsGroups = reportsGrouppedByName.OrderBy(adcr => adcr.Day).ToList();

                    for (var i = 0; i < orderedReportsGroups.Count - 1; i++)
                    {
                        if (orderedReportsGroups[i + 1].Day > orderedReportsGroups[i].Day.AddDays(1))
                        {
                            var total = reportsGrouppedByName.Key == Consts.MainCountryRegion &&
                                        reportsGroupsGrouppedByName.Length > 1
                                ? Metrics.Empty
                                : orderedReportsGroups[i].Total;

                            var missedReport = new LinkedReport(
                                reportsGrouppedByName.Key,
                                orderedReportsGroups[i].Day.AddDays(1),
                                level,
                                total)
                            {
                                Parent = orderedReportsGroups[i].Parent
                            };

                            orderedReportsGroups.Insert(i + 1, missedReport);
                            reports.Add(new LinkedReportWithParent(reportsGroup.Key, missedReport));
                        }

                        orderedReportsGroups[i].Next = orderedReportsGroups[i + 1];
                        orderedReportsGroups[i + 1].Previous = orderedReportsGroups[i];
                    }
                }
            }
        }

        private static void PopulateMissedChildReports(ICollection<LinkedReportWithParent> allreports, IsoLevel level, Func<string, IEnumerable<string>> childrenToCreateFunc)
        {
            foreach (var grouppedReports in allreports
                .Where(lr => lr.Report.Level == level)
                .GroupBy(r => r.Report.Name))

            {
                foreach (var report in grouppedReports.Select(r => r.Report)
                    .OrderBy(r => r.Day))
                {
                    var childrenToCreate = childrenToCreateFunc(grouppedReports.Key).ToList();
                    var missedReports = new List<LinkedReport>();

                    foreach (var child in report.Children)
                    {
                        childrenToCreate.Remove(child.Name);
                    }

                    if (report.Previous != LinkedReport.Empty)
                    {
                        var previousRep = report.Previous;

                        foreach (var child in previousRep.Children)
                        {
                            if (!childrenToCreate.Remove(child.Name))
                            {
                                continue;
                            }

                            if (previousRep.Children.Count > 1 && child.Name == Consts.MainCountryRegion)
                            {
                                continue;
                            }

                            missedReports.Add(child.Copy(report.Day, LinkedReport.Empty));
                        }
                    }

                    missedReports.AddRange(
                        childrenToCreate.Select(pr =>
                            new LinkedReport(pr, report.Day, level + 1, Metrics.Empty)));

                    foreach (var missedReport in missedReports)
                    {
                        allreports.Add(new LinkedReportWithParent(grouppedReports.Key, missedReport));

                        missedReport.Parent = report;
                        report.Children.Add(missedReport);
                    }
                }
            }
        }

        private static void PopulateMissedParentReports(List<LinkedReportWithParent> allReports, IsoLevel level, string parentParentName)
        {
            foreach (var childReports in allReports
                .Where(lr => lr.Report.Level == level)
                .GroupBy(lr => lr.Parent)
                .ToArray())
            {
                var parentsFromChildrenReports = CreateParentReports(childReports, childReports.Key, level - 1);

                allReports.AddRange(
                    parentsFromChildrenReports.Select(rep => new LinkedReportWithParent(parentParentName, rep)));
            }
        }

        /// <summary>
        /// Thoroughly checks reports collection for duplicates. Writes all found duplicates to the logger.
        /// </summary>
        private bool AssertReports(IEnumerable<LinkedReportWithParent> allReports)
        {
            if (Consts.DisableExtensiveAssertMethods)
                return true;

#pragma warning disable 162
            // ReSharper disable once HeuristicUnreachableCode
            var res = 1;
            var reports = allReports.ToArray();

            Parallel.ForEach(reports, rep =>
            {
                var (parent, report) = rep;

                var count = reports.Count(ir =>
                    ir.Report.Day == report.Day &&
                    ir.Report.Level == report.Level &&
                    ir.Report.Name == report.Name &&
                    ir.Parent == parent);

                if (count <= 1)
                    return;

                _logger.WriteError($"{_countryName}: {report.Day:d}-{report.Level}-{report.Name} - duplicates {count}");
                Interlocked.Decrement(ref res);
            });

            return res == 1;
#pragma warning restore 162
        }

        private LinkedReport CreateLinkedReport(BasicReport report)
        {
            LinkedReport newReport;

            if (report.Name == _countryName)
            {
                newReport = new LinkedReport(report.Name, report.Day, IsoLevel.CountryRegion, report.Total);
            }
            else if (report.Parent == _countryName)
            {
                newReport = new LinkedReport(report.Name, report.Day, IsoLevel.ProvinceState, report.Total);
            }
            else
            {
                newReport = new LinkedReport(report.Name, report.Day, IsoLevel.County, report.Total);
            }

            return newReport;
        }

        private readonly struct LinkedReportWithParent
        {
            public LinkedReportWithParent(string parent, LinkedReport report)
            {
                Parent = parent;
                Report = report;
            }

            public string Parent { get; }
            public LinkedReport Report { get; }

            public void Deconstruct(out string parent, out LinkedReport report)
            {
                parent = Parent;
                report = Report;
            }
        }
    }
}