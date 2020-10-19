using System;
using System.Collections.Generic;
using ReportsGenerator.Model.Reports;

namespace ReportsGenerator.Data
{
    public interface IReportFormatter
    {
        IEnumerable<string> GetData(DayReport report, string country);

        IEnumerable<string> GetData(BaseCountryReport report, DateTime day);

        IEnumerable<string> GetData(StatsReportNode report);

        IEnumerable<string> GetHeader(DayReport report);

        IEnumerable<string> GetHeader(BaseCountryReport report);

        IEnumerable<string> GetHeader(StatsReportNode report);

        IEnumerable<string> GetName(DayReport report);

        public IEnumerable<string> GetName(BaseCountryReport report, string? parent = null);

        IEnumerable<string> GetName(StatsReportNode report);
    }
}