using System;
using System.Collections.Generic;
using System.Linq;

namespace CovidSandbox.Model.Reports.Intermediate
{
    public class ReportsGraphBuilder
    {
        private readonly string _countryName;
        private readonly ReportsGraphStructure _graphStructure;

        public ReportsGraphBuilder(string countryName)
        {
            _countryName = countryName;
            _graphStructure = new ReportsGraphStructure("test");
            Reports = new List<BasicReport>();
        }

        public ICollection<BasicReport> Reports { get; }

        public LinkedReport Build(ReportsGraphStructure structure)
        {
            var allReports =
                Reports.Select(br => new LinkedReportWithParent(br.Parent, CreateLinkedReport(br))).ToList();

            ProcessReportsLevel(allReports, IsoLevel.County, _countryName);
            ProcessReportsLevel(allReports, IsoLevel.ProvinceState, string.Empty);

            FillNextPrevReports(allReports, IsoLevel.CountryRegion);

            PopulateMissedReports(allReports, IsoLevel.ProvinceState, name => structure.GetProvinces());
            FillNextPrevReports(allReports, IsoLevel.ProvinceState);

            PopulateMissedReports(allReports, IsoLevel.County, structure.GetCounties);
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

        private static void FillNextPrevReports(IEnumerable<LinkedReportWithParent> reports, IsoLevel level)
        {
            foreach (var reportsGroup in reports
                .Where(lr => lr.Report.Level == level)
                .GroupBy(lr => lr.Parent))
            {
                foreach (var reportsGrouppedByDay in reportsGroup.Select(rg => rg.Report).GroupBy(pcr => pcr.Name))
                {
                    var orderedReportsGroups = reportsGrouppedByDay.OrderBy(adcr => adcr.Day).ToArray();

                    for (var i = 0; i < orderedReportsGroups.Length - 1; i++)
                    {
                        orderedReportsGroups[i].Next = orderedReportsGroups[i + 1];
                        orderedReportsGroups[i + 1].Previous = orderedReportsGroups[i];
                    }
                }
            }
        }

        private static void ProcessReportsLevel(List<LinkedReportWithParent> allReports, IsoLevel level, string parentParentName)
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

        private static void PopulateMissedReports(ICollection<LinkedReportWithParent> allreports, IsoLevel level,Func<string,IEnumerable<string>> childrenToCreateFunc)
        {
            foreach (var grouppedReports in allreports
                .Where(lr => lr.Report.Level == level-1)
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
                            new LinkedReport(pr, report.Day, level, Metrics.Empty)));

                    foreach (var missedReport in missedReports)
                    {
                        allreports.Add(new LinkedReportWithParent(grouppedReports.Key, missedReport));

                        missedReport.Parent = report;
                        report.Children.Add(missedReport);
                    }
                }
            }
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