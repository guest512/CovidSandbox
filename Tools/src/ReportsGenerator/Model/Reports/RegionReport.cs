using System;
using System.Collections.Generic;
using System.Linq;
using ReportsGenerator.Model.Reports.Intermediate;

namespace ReportsGenerator.Model.Reports
{
    public class RegionReport : BaseCountryReport

    {
        public RegionReport(string name, LinkedReport head) : base(head, name)
        {
        }
    }
}