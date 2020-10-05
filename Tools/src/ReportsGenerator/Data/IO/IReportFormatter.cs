using ReportsGenerator.Model.Reports;
using System;
using System.Collections.Generic;

namespace ReportsGenerator.Data.IO
{
    public interface IReportFormatter
    {
        IEnumerable<string> GetData(DayReport report, string country);

        IEnumerable<string> GetData(BaseCountryReport report, DateTime country);

        IEnumerable<string> GetHeader(DayReport report);

        IEnumerable<string> GetHeader(BaseCountryReport report);

        IEnumerable<string> GetName(DayReport report);

        public IEnumerable<string> GetName(BaseCountryReport report, string? parent = null);
    }
}