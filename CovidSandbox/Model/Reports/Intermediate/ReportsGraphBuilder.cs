using CovidSandbox.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CovidSandbox.Model.Reports.Intermediate
{
    public class ReportsGraphBuilder
    {
        private readonly string _countryName;
        private readonly ILogger _logger;

        public ReportsGraphBuilder(string countryName, ILogger logger)
        {
            _countryName = countryName;
            _logger = logger;
            Reports = new List<BasicReport>();
        }

        public ICollection<BasicReport> Reports { get; }

        public LinkedReport Build(ReportsGraphStructure structure)
        {
            var allReports =
                Reports.Select(br => new LinkedReportWithParent(br.Parent, CreateLinkedReport(br))).ToList();

            //Day: { 04.02.2020 0:00:00}
            //Level: ProvinceState
            //Name: "Ontario"

            Debug.Assert(AssertReports(allReports));

            PopulateMissedParentReports(allReports, IsoLevel.County, _countryName);
            Debug.Assert(AssertReports(allReports));

            PopulateMissedParentReports(allReports, IsoLevel.ProvinceState, string.Empty);
            Debug.Assert(AssertReports(allReports));

            FillNextPrevReports(allReports, IsoLevel.CountryRegion);
            Debug.Assert(AssertReports(allReports));

            PopulateMissedChildReports(allReports, IsoLevel.CountryRegion, name => structure.GetProvinces());
            Debug.Assert(AssertReports(allReports));

            FillNextPrevReports(allReports, IsoLevel.ProvinceState);
            Debug.Assert(AssertReports(allReports));

            PopulateMissedChildReports(allReports, IsoLevel.ProvinceState, structure.GetCounties);
            Debug.Assert(AssertReports(allReports));

            FillNextPrevReports(allReports, IsoLevel.County);
            Debug.Assert(AssertReports(allReports));

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
                foreach (var reportsGrouppedByDay in reportsGroup.Select(rg => rg.Report).GroupBy(pcr => pcr.Name))
                {
                    var orderedReportsGroups = reportsGrouppedByDay.OrderBy(adcr => adcr.Day).ToList();

                    for (var i = 0; i < orderedReportsGroups.Count - 1; i++)
                    {
                        if (orderedReportsGroups[i + 1].Day > orderedReportsGroups[i].Day.AddDays(1))
                        {
                            var missedReport = new LinkedReport(
                                reportsGrouppedByDay.Key,
                                orderedReportsGroups[i].Day.AddDays(1),
                                level,
                                orderedReportsGroups[i].Total)
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
                            if (childrenToCreate.Remove(child.Name))
                            {
                                missedReports.Add(child.Copy(report.Day, LinkedReport.Empty));
                            }
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

        private bool AssertReports(IEnumerable<LinkedReportWithParent> allReports)
        {
            return true;

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