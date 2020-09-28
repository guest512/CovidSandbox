using System;
using System.Collections.Generic;
using System.Linq;

namespace CovidSandbox.Model.Reports.Intermediate
{
    public class ReportsGraphBuilder
    {
        private readonly string _countryName;

        public ReportsGraphBuilder(string countryName)
        {
            _countryName = countryName;
            Reports = new List<BasicReport>();
        }

        public ICollection<BasicReport> Reports { get; }

        public LinkedReport Build(ReportsGraphStructure structure)
        {
            var allReports =
                Reports.Select(br => new LinkedReportWithParent(br.Parent, CreateLinkedReport(br))).ToList();

            PopulateMissedParentReports(allReports, IsoLevel.County, _countryName);
            PopulateMissedParentReports(allReports, IsoLevel.ProvinceState, string.Empty);


            FillNextPrevReports(allReports, IsoLevel.CountryRegion);
            PopulateMissedChildReports(allReports, IsoLevel.CountryRegion, name => structure.GetProvinces());

            FillNextPrevReports(allReports, IsoLevel.ProvinceState);
            PopulateMissedChildReports(allReports, IsoLevel.ProvinceState, structure.GetCounties);

            FillNextPrevReports(allReports, IsoLevel.County);

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